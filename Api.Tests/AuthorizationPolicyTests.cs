extern alias VetApi;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using VetControl.Testing;
using Xunit;
using ClinicalPolicies = VetApi::Api.Security.ClinicalPolicies;

namespace Api.Tests;

public sealed class AuthorizationPolicyTests
{
    [Theory]
    [InlineData(ClinicalPolicies.ManageAnimals, true, true, true, false, true)]
    [InlineData(ClinicalPolicies.ManageSpeciesAndBreeds, true, true, true, false, true)]
    [InlineData(ClinicalPolicies.ManageVaccines, true, true, false, false, true)]
    [InlineData(ClinicalPolicies.ManageProtocols, true, true, false, false, false)]
    [InlineData(ClinicalPolicies.ManageMovements, true, true, true, false, true)]
    [InlineData(ClinicalPolicies.ManageVaccineApplications, true, true, false, true, true)]
    [InlineData(ClinicalPolicies.DeleteVaccineApplications, true, true, false, false, false)]
    [InlineData(ClinicalPolicies.SignVaccineApplications, false, true, false, false, false)]
    public async Task PoliciesEnforceTheRoleMatrixAndRequireAuthentication(string policy,
        bool admin, bool veterinarian, bool handler, bool applicator, bool intern)
    {
        // Resolving the real policy engine needs no database connection or provider server.
        await using var database = new PostgresTestDatabase("api");
        await using var factory = new ClinicalApiFactory(database);
        using var scope = factory.Services.CreateScope();
        var authorization = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        var roles = new[] { "admin", "medico_veterinario", "manejador_animal", "aplicador", "estagiario_medvet" };
        var expected = new[] { admin, veterinarian, handler, applicator, intern };
        for (var index = 0; index < roles.Length; index++)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("role", roles[index])], "test", "name", "role"));
            Assert.Equal(expected[index], (await authorization.AuthorizeAsync(user, null, policy)).Succeeded);
        }
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity([new Claim("role", "medico_veterinario")], null, "name", "role"));
        Assert.False((await authorization.AuthorizeAsync(anonymous, null, policy)).Succeeded);
        var unknown = new ClaimsPrincipal(new ClaimsIdentity([new Claim("role", "unknown")], "test", "name", "role"));
        Assert.False((await authorization.AuthorizeAsync(unknown, null, policy)).Succeeded);
    }
}
