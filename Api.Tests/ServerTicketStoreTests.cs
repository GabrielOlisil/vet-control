extern alias VetApi;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ApiContext = VetApi::Api.Database.ApiContext;
using ServerTicketStore = VetApi::Api.Security.ServerTicketStore;

namespace Api.Tests;

public sealed class ServerTicketStoreTests(ClinicalFixture fixture) : IClassFixture<ClinicalFixture>
{
    [Fact]
    public async Task ProtectedSessionsAreSharedByStoreInstancesAndRemovalIsPersisted()
    {
        var first = fixture.Factory.Services.GetRequiredService<ServerTicketStore>();
        var second = new ServerTicketStore(fixture.Factory.Services.GetRequiredService<IServiceScopeFactory>(),
            fixture.Factory.Services.GetRequiredService<TicketDataFormat>());
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "Session owner")], "test"));
        var properties = new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) };
        properties.StoreTokens([new AuthenticationToken { Name = "access_token", Value = "sensitive-test-token" }]);
        var ticket = new AuthenticationTicket(principal, properties, CookieAuthenticationDefaults.AuthenticationScheme);
        var key = await first.StoreAsync(ticket);

        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        var stored = await db.ServerSessions.AsNoTracking().SingleAsync(x => x.Id == key);
        Assert.DoesNotContain("sensitive-test-token", stored.ProtectedTicket);
        var recovered = await second.RetrieveAsync(key);
        Assert.NotNull(recovered);
        Assert.Equal("Session owner", recovered.Principal.Identity!.Name);
        Assert.Equal("sensitive-test-token", recovered.Properties.GetTokenValue("access_token"));

        recovered.Properties.UpdateTokenValue("access_token", "renewed-test-token");
        await second.RenewAsync(key, recovered);
        Assert.Equal("renewed-test-token", (await first.RetrieveAsync(key))!.Properties.GetTokenValue("access_token"));
        await first.RemoveAsync(key);
        await second.RenewAsync(key, recovered);
        Assert.Null(await second.RetrieveAsync(key));
        Assert.False(await db.ServerSessions.AnyAsync(x => x.Id == key));
    }

    [Fact]
    public async Task RefreshLockIsSharedAcrossIndependentStoreInstances()
    {
        var first = fixture.Factory.Services.GetRequiredService<ServerTicketStore>();
        var second = new ServerTicketStore(fixture.Factory.Services.GetRequiredService<IServiceScopeFactory>(),
            fixture.Factory.Services.GetRequiredService<TicketDataFormat>());
        var key = "vet-session:" + Guid.NewGuid().ToString("N");
        var lease = await first.AcquireAsync(key);
        var waiting = second.AcquireAsync(key);
        try
        {
            await Assert.ThrowsAsync<TimeoutException>(() => waiting.WaitAsync(TimeSpan.FromMilliseconds(200)));
        }
        finally { await lease.DisposeAsync(); }
        await using var acquired = await waiting.WaitAsync(TimeSpan.FromSeconds(5));
        await using var unrelated = await first.AcquireAsync(key + ":another").WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ExpiredOrCorruptSessionCannotBeRetrieved()
    {
        var store = fixture.Factory.Services.GetRequiredService<ServerTicketStore>();
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiContext>();
        var id = "vet-session:" + Guid.NewGuid().ToString("N");
        db.ServerSessions.Add(new VetApi::Api.Models.ServerSession
        {
            Id = id, ProtectedTicket = "invalid-payload", ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        });
        await db.SaveChangesAsync();
        Assert.Null(await store.RetrieveAsync(id));
        await db.ServerSessions.Where(x => x.Id == id).ExecuteUpdateAsync(setters =>
            setters.SetProperty(x => x.ExpiresAt, DateTimeOffset.UtcNow.AddMinutes(-1)));
        Assert.Null(await store.RetrieveAsync(id));
    }
}
