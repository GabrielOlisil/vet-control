using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Configuration;

public sealed class ServerSessionConfiguration : IEntityTypeConfiguration<ServerSession>
{
    public void Configure(EntityTypeBuilder<ServerSession> builder)
    {
        builder.ToTable("ServerSessions");
        builder.HasKey(session => session.Id);
        builder.Property(session => session.Id).HasMaxLength(100).ValueGeneratedNever();
        builder.Property(session => session.ProtectedTicket).IsRequired();
        builder.Property(session => session.ExpiresAt).IsRequired();
        builder.HasIndex(session => session.ExpiresAt);
    }
}
