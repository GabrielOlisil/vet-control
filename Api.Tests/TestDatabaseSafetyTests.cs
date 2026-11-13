using VetControl.Testing;
using Xunit;

namespace Api.Tests;

public sealed class TestDatabaseSafetyTests
{
    [Theory]
    [InlineData("vacinas")]
    [InlineData("AppUsers")]
    [InlineData("postgres")]
    [InlineData("vet_api_tests_")]
    [InlineData("vet_api_tests_0123456789abcdef0123456789abcdef\n")]
    [InlineData("vet_api_tests_0123456789abcdef0123456789abcdef\"; DROP DATABASE vacinas;")]
    public void RejectsAnyNonDisposableDatabaseName(string name) =>
        Assert.Throws<InvalidOperationException>(() => PostgresTestDatabase.ValidateName(name));

    [Fact]
    public async Task CleanupRemovesOnlyTheDatabaseOwnedByTheInstance()
    {
        await using var first = new PostgresTestDatabase("api");
        await using var second = new PostgresTestDatabase("api");
        Assert.NotEqual(first.Name, second.Name);
        await first.CreateAsync();
        await second.CreateAsync();
        Assert.True(await first.ExistsAsync());
        Assert.True(await second.ExistsAsync());
        await first.DisposeAsync();
        Assert.False(await first.ExistsAsync());
        Assert.True(await second.ExistsAsync());
        await first.DisposeAsync(); // Cleanup is idempotent.
    }

    [Fact]
    public void RejectsUnknownAreasBeforeConnecting() =>
        Assert.Throws<ArgumentException>(() => new PostgresTestDatabase("production"));
}
