extern alias VetApi;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VetControl.Testing;
using Xunit;
using ApiContext = VetApi::Api.Database.ApiContext;
using Animal = VetApi::Api.Models.Animal;

namespace Api.Tests;

public sealed class DevelopmentSeedTests
{
    [Fact]
    public async Task RepeatingSeedPreservesDemoEditsAndDoesNotDuplicateRecords()
    {
        await using var database = new PostgresTestDatabase("api");
        await database.CreateAsync();
        await using var db = Context(database);
        await db.Database.MigrateAsync();
        await RunSeed(database);
        var animal = await db.Animals.OrderBy(x => x.Id).FirstAsync();
        animal.Name = "Nome alterado durante a apresentação";
        await db.SaveChangesAsync();
        var signature = await db.AplicacoesVacina.Where(x => x.HashAssinatura != null)
            .Select(x => x.HashAssinatura).SingleAsync();

        await RunSeed(database);

        Assert.Equal(50, await db.Animals.CountAsync());
        Assert.Equal(100, await db.IdentificadorAnimal.CountAsync());
        Assert.Equal(20, await db.Vacinas.CountAsync());
        Assert.Equal(3, await db.ProtocolosVacinais.CountAsync());
        Assert.Equal(53, await db.MovimentacoesAnimais.CountAsync());
        Assert.Equal(6, await db.AplicacoesVacina.CountAsync());
        Assert.Equal(1, await db.AssinaturasAplicacoesVacina.CountAsync());
        Assert.Equal(animal.Name, (await db.Animals.AsNoTracking().SingleAsync(x => x.Id == animal.Id)).Name);
        Assert.Equal(signature, await db.AplicacoesVacina.Where(x => x.HashAssinatura != null)
            .Select(x => x.HashAssinatura).SingleAsync());
        Assert.Empty(await db.ServerSessions.ToListAsync());
    }

    [Fact]
    public async Task ConcurrentSeedExecutionsOnlyInitializeOnce()
    {
        await using var database = new PostgresTestDatabase("api");
        await database.CreateAsync();
        await using var db = Context(database);
        await db.Database.MigrateAsync();

        await Task.WhenAll(RunSeed(database), RunSeed(database));

        Assert.Equal(50, await db.Animals.CountAsync());
        Assert.Equal(53, await db.MovimentacoesAnimais.CountAsync());
    }

    [Fact]
    public async Task OccupiedDatabaseWithoutSeedMarkerIsRejectedWithoutChangingExistingData()
    {
        await using var database = new PostgresTestDatabase("api");
        await database.CreateAsync();
        await using var db = Context(database);
        await db.Database.MigrateAsync();
        var existing = new Animal { Id = Guid.NewGuid(), Name = "Cadastro que precisa ser preservado", DataNascimento = new DateOnly(2020, 1, 1) };
        db.Animals.Add(existing);
        await db.SaveChangesAsync();

        var failure = await Assert.ThrowsAsync<PostgresException>(() => RunSeed(database));

        Assert.Contains("ocupada", failure.MessageText);
        Assert.Equal(existing.Id, (await db.Animals.SingleAsync()).Id);
        Assert.Empty(await db.Especies.ToListAsync());
    }

    [Fact]
    public async Task FailedSeedRollsBackAllRecordsAndCanBeRetried()
    {
        await using var database = new PostgresTestDatabase("api");
        await database.CreateAsync();
        await using var db = Context(database);
        await db.Database.MigrateAsync();
        var script = await ReadSeed();
        // Fail after the catalog and animals have already been inserted.
        var broken = script.Replace("INSERT INTO \"AplicacoesVacina\"", "INSERT INTO \"MissingSeedTable\"");
        Assert.NotEqual(script, broken);

        await Assert.ThrowsAsync<PostgresException>(() => RunSeed(database, broken));

        Assert.Empty(await db.Animals.ToListAsync());
        Assert.Empty(await db.Especies.ToListAsync());
        await RunSeed(database);
        Assert.Equal(50, await db.Animals.CountAsync());
    }

    [Fact]
    public async Task DemoHasConsistentMovementHistoryAndBothUpcomingAndOverdueVaccinations()
    {
        await using var database = new PostgresTestDatabase("api");
        await database.CreateAsync();
        await using var db = Context(database);
        await db.Database.MigrateAsync();
        await using (var connection = new NpgsqlConnection(database.ConnectionString))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand("SET vet_control.seed_today = '2026-11-10';" + await ReadSeed(), connection);
            await command.ExecuteNonQueryAsync();
        }

        foreach (var animal in await db.Animals.Include(x => x.Movimentacoes).ToListAsync())
        {
            var history = animal.Movimentacoes.OrderBy(x => x.DataMovimentacao).ThenBy(x => x.CreationDateTime).ToList();
            Assert.Equal(0, (int)history[0].Tipo);
            Assert.Equal(animal.LoteOuPasto, history[^1].LocalDestino);
            Assert.All(history, x => Assert.True(x.DataMovimentacao <= new DateOnly(2026, 11, 10)));
        }
        var due = await db.AnimalProtocolosVacinais.Include(x => x.ProtocoloVacinal).ThenInclude(x => x!.Doses)
            .SelectMany(x => x.ProtocoloVacinal!.Doses.Select(dose => x.DataInicio.AddDays(dose.DiasAposInicio))).ToListAsync();
        Assert.Contains(due, x => x < new DateOnly(2026, 11, 10));
        Assert.Contains(due, x => x >= new DateOnly(2026, 11, 10) && x <= new DateOnly(2026, 11, 17));
    }

    [Fact]
    public async Task SeedRefusesUnapprovedDatabaseBeforeAccessingClinicalTables()
    {
        // This changes only the reported name in the script, never the connection target.
        await using var database = new PostgresTestDatabase("api");
        await database.CreateAsync();
        var script = (await ReadSeed()).Replace("current_database()", "'vacinas'::text");

        var failure = await Assert.ThrowsAsync<PostgresException>(() => RunSeed(database, script));

        Assert.Contains("ambiente de demonstração", failure.MessageText);
    }

    private static ApiContext Context(PostgresTestDatabase database) => new(
        new DbContextOptionsBuilder<ApiContext>().UseNpgsql(database.ConnectionString).Options, new HttpContextAccessor());

    private static async Task RunSeed(PostgresTestDatabase database, string? script = null)
    {
        await using var connection = new NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(script ?? await ReadSeed(), connection) { CommandTimeout = 60 };
        await command.ExecuteNonQueryAsync();
    }

    private static Task<string> ReadSeed()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "seed.sql"))) directory = directory.Parent;
        return File.ReadAllTextAsync(Path.Combine(directory?.FullName ?? throw new InvalidOperationException("Repository not found."), "seed.sql"));
    }
}
