using System.Text.RegularExpressions;
using Npgsql;

namespace VetControl.Testing;

/// <summary>Owns exactly one randomly named database; application databases are never reset.</summary>
public sealed partial class PostgresTestDatabase : IAsyncDisposable
{
    private readonly string adminConnection;
    private bool created;

    public PostgresTestDatabase(string area)
    {
        if (area is not ("auth" or "api" or "e2e"))
            throw new ArgumentException("Unknown test database area.", nameof(area));

        Name = $"vet_{area}_tests_{Guid.NewGuid():N}";
        ValidateName(Name);
        var settings = new NpgsqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable("VET_TEST_POSTGRES") ??
            "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgresssss")
        {
            Database = "postgres",
            Pooling = false,
            Timeout = 10,
            CommandTimeout = 30
        };
        adminConnection = settings.ConnectionString;
        settings.Database = Name;
        ConnectionString = settings.ConnectionString;
    }

    public string Name { get; }
    public string ConnectionString { get; }

    public static void ValidateName(string name)
    {
        if (!DisposableDatabaseName().IsMatch(name))
            throw new InvalidOperationException("Refusing to operate on a non-disposable test database.");
    }

    public async Task CreateAsync()
    {
        if (created) throw new InvalidOperationException("The test database already exists.");
        ValidateTarget();
        await using var connection = new NpgsqlConnection(adminConnection);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE DATABASE \"{Name}\"", connection);
        await command.ExecuteNonQueryAsync();
        created = true;
    }

    public async Task<bool> ExistsAsync()
    {
        await using var connection = new NpgsqlConnection(adminConnection);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("SELECT EXISTS (SELECT 1 FROM pg_database WHERE datname = $1)", connection);
        command.Parameters.AddWithValue(Name);
        return (bool)(await command.ExecuteScalarAsync())!;
    }

    public async ValueTask DisposeAsync()
    {
        if (!created) return;
        ValidateTarget();
        await using var connection = new NpgsqlConnection(adminConnection);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE \"{Name}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
        created = false;
    }

    private void ValidateTarget()
    {
        ValidateName(Name);
        if (new NpgsqlConnectionStringBuilder(ConnectionString).Database != Name)
            throw new InvalidOperationException("Test database connection does not match its owner.");
    }

    [GeneratedRegex("\\Avet_(auth|api|e2e)_tests_[a-f0-9]{32}\\z", RegexOptions.CultureInvariant)]
    private static partial Regex DisposableDatabaseName();
}
