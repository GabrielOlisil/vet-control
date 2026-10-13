using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

namespace Api.Security;

// Browser cookies only contain an opaque session id. OIDC tokens stay in this server-side store.
// Replace with a distributed ITicketStore before running multiple API instances.
public sealed class ServerTicketStore(IMemoryCache cache) : ITicketStore
{
    public sealed record Session(AuthenticationTicket Ticket, SemaphoreSlim Gate);
    public Session? Find(string key) => cache.Get<Session>(key);
    public Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = "vet-session:" + Guid.NewGuid().ToString("N");
        ticket.Properties.Items["session-key"] = key;
        cache.Set(key, new Session(ticket, new SemaphoreSlim(1)), ticket.Properties.ExpiresUtc ?? DateTimeOffset.UtcNow.AddHours(8));
        return Task.FromResult(key);
    }
    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var gate = Find(key)?.Gate ?? new SemaphoreSlim(1);
        cache.Set(key, new Session(ticket, gate), ticket.Properties.ExpiresUtc ?? DateTimeOffset.UtcNow.AddHours(8));
        return Task.CompletedTask;
    }
    public Task<AuthenticationTicket?> RetrieveAsync(string key) => Task.FromResult(Find(key)?.Ticket);
    public Task RemoveAsync(string key) { cache.Remove(key); return Task.CompletedTask; }
}
