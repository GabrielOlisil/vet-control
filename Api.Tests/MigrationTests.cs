extern alias VetApi;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VetControl.Testing;
using Xunit;
using ApiContext = VetApi::Api.Database.ApiContext;
using ServerSession = VetApi::Api.Models.ServerSession;

namespace Api.Tests;

public sealed class MigrationTests
{
    [Fact]
    public async Task CurrentBaselineCanBeUpgradedWithoutLosingExistingAnimals()
    {
        await using var database = new PostgresTestDatabase("api");
        await database.CreateAsync();
        await using var db = new ApiContext(new DbContextOptionsBuilder<ApiContext>()
            .UseNpgsql(database.ConnectionString).Options, new HttpContextAccessor());
        await db.GetService<IMigrator>().MigrateAsync("20261014150710_InitialCreate");
        var animalId = Guid.NewGuid();
        var name = "Animal anterior ao upgrade";
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Animals" ("Id", "Name", "DataNascimento", "DataNascimentoAproximada", "LoteOuPasto", "Sexo", "OrigemAnimal", "Ativo", "CreationDateTime")
            VALUES ({animalId}, {name}, DATE '2020-01-01', false, ' Pasto de Teste ', 'Indefinido', 1, true, now())
            """);

        await db.Database.MigrateAsync();
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        var animal = await db.Animals.AsNoTracking().Where(x => x.Id == animalId)
            .Select(x => new { x.Name, x.LoteOuPasto }).SingleAsync();
        Assert.Equal(name, animal.Name);
        Assert.Equal(" Pasto de Teste ", animal.LoteOuPasto);
        var catalog = await db.Database.SqlQueryRaw<string>("""SELECT "Nome" AS "Value" FROM "Ambientes" a JOIN "Animals" n ON a."Id" = n."AmbienteId" """).SingleAsync();
        Assert.Equal("Pasto de Teste", catalog);
        // Exercise the newly introduced table rather than just asserting migration names.
        db.ServerSessions.Add(new ServerSession
        {
            Id = "vet-session:migration-test", ProtectedTicket = "test-payload", ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        });
        await db.SaveChangesAsync();
        Assert.Equal(1, await db.ServerSessions.CountAsync());
    }
}
