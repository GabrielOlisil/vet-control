using Api.Database;
using Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;

namespace Api.Security;

// Keeps the browser cookie opaque while making the ticket available to every API instance.
// The protected payload is stored in PostgreSQL; all instances must share Data Protection keys.
public sealed class ServerTicketStore(
    IServiceScopeFactory scopeFactory,
    TicketDataFormat ticketFormat) : ITicketStore
{
    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = "vet-session:" + Guid.NewGuid().ToString("N");
        ticket.Properties.Items["session-key"] = key;
        var expiresAt = GetExpiry(ticket);

        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApiContext>();
        context.ServerSessions.Add(new ServerSession
        {
            Id = key,
            ProtectedTicket = ticketFormat.Protect(ticket),
            ExpiresAt = expiresAt
        });
        await context.SaveChangesAsync();
        return key;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var expiresAt = GetExpiry(ticket);
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApiContext>();
        var protectedTicket = ticketFormat.Protect(ticket);
        // A refresh finishing after logout must not recreate the removed session.
        await context.ServerSessions.Where(x => x.Id == key && x.ExpiresAt > DateTimeOffset.UtcNow)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ProtectedTicket, protectedTicket)
                .SetProperty(x => x.ExpiresAt, expiresAt));
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApiContext>();
        var session = await context.ServerSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == key);
        if (session is null || session.ExpiresAt <= DateTimeOffset.UtcNow)
            return null;

        try
        {
            return ticketFormat.Unprotect(session.ProtectedTicket);
        }
        catch (Exception) when (session.ProtectedTicket.Length > 0)
        {
            return null;
        }
    }

    public async Task RemoveAsync(string key)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApiContext>();
        await context.ServerSessions.Where(x => x.Id == key).ExecuteDeleteAsync();
    }

    public async Task<IAsyncDisposable> AcquireAsync(string key, CancellationToken ct = default)
    {
        var scope = scopeFactory.CreateAsyncScope();
        IDbContextTransaction? transaction = null;
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<ApiContext>();
            transaction = await context.Database.BeginTransactionAsync(ct);
            var lockId = BinaryPrimitives.ReadInt64BigEndian(SHA256.HashData(Encoding.UTF8.GetBytes("VetControl.Refresh:" + key)));
            await context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({lockId})", ct);
            return new SessionLease(scope, transaction);
        }
        catch
        {
            if (transaction is not null) await transaction.DisposeAsync();
            await scope.DisposeAsync();
            throw;
        }
    }

    private sealed class SessionLease(AsyncServiceScope scope, IDbContextTransaction transaction) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            try { await transaction.DisposeAsync(); }
            finally { await scope.DisposeAsync(); }
        }
    }

    private static DateTimeOffset GetExpiry(AuthenticationTicket ticket) =>
        ticket.Properties.ExpiresUtc ?? DateTimeOffset.UtcNow.AddHours(8);
}
