namespace Api.Models;

public sealed class ServerSession
{
    public string Id { get; set; } = string.Empty;
    public string ProtectedTicket { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
}
