extern alias VetApi;
using System.Net;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Auth.Contracts;
using Auth.Database;
using Auth.Models;
using Auth.Security;
using Auth.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using Xunit;

namespace Auth.Tests;

public sealed class AuthenticationTests : IAsyncLifetime
{
    private readonly string database = "vet_auth_tests_" + Guid.NewGuid().ToString("N");
    private string connection = "";
    private AuthFactory factory = null!;
    private HttpClient admin = null!;
    private bool created;
    private const string Password = "Integration-Test1!";

    public async Task InitializeAsync()
    {
        var setup = new NpgsqlConnectionStringBuilder(AuthContext.ConnectionString) { Database = "postgres" };
        await using var connectionAdmin = new NpgsqlConnection(setup.ConnectionString);
        await connectionAdmin.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", connectionAdmin)) await command.ExecuteNonQueryAsync();
        created = true;
        setup.Database = database; connection = setup.ConnectionString;
        await using var db = new AuthContext(new DbContextOptionsBuilder<AuthContext>().UseNpgsql(connection).UseOpenIddict().Options);
        await db.Database.MigrateAsync();
        factory = new AuthFactory(connection);
        admin = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("http://localhost:8081"), AllowAutoRedirect = false });
        Assert.True((await admin.GetFromJsonAsync<SetupStatusResponse>("/api/auth/setup"))!.Required);
        var setupResponse = await Mutate(admin, HttpMethod.Post, "/api/auth/setup",
            new InitialSetupRequest("Admin de teste", "testadmin", "admin@test.local", Password, Password, null));
        Assert.Equal(HttpStatusCode.OK, setupResponse.StatusCode);
        Assert.False((await admin.GetFromJsonAsync<SetupStatusResponse>("/api/auth/setup"))!.Required);
    }

    public async Task DisposeAsync()
    {
        admin?.Dispose(); if (factory is not null) await factory.DisposeAsync();
        if (!created) return;
        // Only delete the unique database created by this fixture, never AppUsers or vacinas.
        Assert.Matches("^vet_auth_tests_[a-f0-9]{32}$", database);
        NpgsqlConnection.ClearAllPools();
        var setup = new NpgsqlConnectionStringBuilder(AuthContext.ConnectionString) { Database = "postgres" };
        await using var connectionAdmin = new NpgsqlConnection(setup.ConnectionString); await connectionAdmin.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", connectionAdmin);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<HttpResponseMessage> Mutate(HttpClient client, HttpMethod method, string path, object body)
    {
        var session = (await client.GetFromJsonAsync<AuthSessionResponse>("/api/auth/session"))!;
        var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-CSRF-TOKEN", session.CsrfToken);
        return await client.SendAsync(request);
    }
    private static Task<HttpResponseMessage> Login(HttpClient client, string login, string password) =>
        Mutate(client, HttpMethod.Post, "/api/auth/login", new LoginRequest(login, password, null));

    private async Task<UserResponse> Create(string username, string[] roles, bool mustChange = false)
    {
        var response = await Mutate(admin, HttpMethod.Post, "/api/auth/admin/users", new CreateUserRequest(username, username, username + "@test.local", Password, roles, MustChangePassword: mustChange));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<UserResponse>())!;
    }

    [Fact]
    public async Task FirstLoginCreatesOnlyOneAdministratorAndClosesSetupPermanently()
    {
        // Reset only the users of this fixture's unique, disposable test database.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuthContext>();
            Assert.Equal(database, new NpgsqlConnectionStringBuilder(db.Database.GetConnectionString()).Database);
            await db.Users.ExecuteDeleteAsync();
        }
        var first = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("http://localhost:8081"), AllowAutoRedirect = false });
        var second = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("http://localhost:8081"), AllowAutoRedirect = false });
        Assert.True((await first.GetFromJsonAsync<SetupStatusResponse>("/api/auth/setup"))!.Required);
        var request = new InitialSetupRequest("Primeiro administrador", "firstadmin", "first@test.local", Password, Password,
            "/connect/authorize?client_id=vet-control&state=first-login-state");
        var missingCsrf = await first.PostAsJsonAsync("/api/auth/setup", request);
        Assert.Equal(HttpStatusCode.BadRequest, missingCsrf.StatusCode);
        var mismatch = await Mutate(first, HttpMethod.Post, "/api/auth/setup", request with { ConfirmPassword = "Different-Test2!" });
        Assert.Equal(HttpStatusCode.BadRequest, mismatch.StatusCode);
        var weak = await Mutate(first, HttpMethod.Post, "/api/auth/setup", request with { Password = "weakpassword", ConfirmPassword = "weakpassword" });
        Assert.Equal(HttpStatusCode.BadRequest, weak.StatusCode);
        Assert.True((await first.GetFromJsonAsync<SetupStatusResponse>("/api/auth/setup"))!.Required);

        var responses = await Task.WhenAll(
            Mutate(first, HttpMethod.Post, "/api/auth/setup", request),
            Mutate(second, HttpMethod.Post, "/api/auth/setup", request with { UserName = "secondadmin", Email = "second@test.local" }));
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        Assert.False((await first.GetFromJsonAsync<SetupStatusResponse>("/api/auth/setup"))!.Required);
        var winner = responses[0].IsSuccessStatusCode ? first : second;
        var result = (await responses.Single(x => x.IsSuccessStatusCode).Content.ReadFromJsonAsync<LoginResponse>())!;
        Assert.Contains("state=first-login-state", result.NextUrl);
        var session = (await winner.GetFromJsonAsync<AuthSessionResponse>("/api/auth/session"))!;
        Assert.True(session.Authenticated); Assert.Equal([RoleCatalog.Admin], session.Roles);
        Assert.False(session.MustChangePassword);
        Assert.Equal(HttpStatusCode.OK, (await winner.GetAsync("/api/auth/admin/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Mutate(winner, HttpMethod.Post, "/api/auth/setup", request)).StatusCode);
        using var verification = factory.Services.CreateScope();
        var context = verification.ServiceProvider.GetRequiredService<AuthContext>();
        Assert.Equal(1, await context.Users.CountAsync());
        var user = await context.Users.SingleAsync();
        Assert.NotEqual(Password, user.PasswordHash); Assert.NotEqual(default, user.CreationDateTime);
        Assert.True(await context.AuditEvents.AnyAsync(x => x.UserId == user.Id && x.ActorId == user.Id && x.Action == "user.bootstrap"));
        // Setup stays closed even if the only account becomes inactive outside the administration UI.
        user.IsActive = false; await context.SaveChangesAsync();
        Assert.False((await second.GetFromJsonAsync<SetupStatusResponse>("/api/auth/setup"))!.Required);
        Assert.Equal(HttpStatusCode.Conflict, (await Mutate(second, HttpMethod.Post, "/api/auth/setup", request)).StatusCode);
    }

    [Fact]
    public async Task AdminOnly_Csrf_Hierarchy_AndLastAdminAreEnforced()
    {
        var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/auth/admin/users")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsJsonAsync("/api/auth/admin/users", new { })).StatusCode);
        var intern = await Create("intern", [RoleCatalog.Intern]);
        Assert.Equal(new[] { RoleCatalog.Applicator, RoleCatalog.Intern, RoleCatalog.Handler }.Order(), intern.EffectiveRoles);
        Assert.NotEqual(default, intern.CreationDateTime);
        var list = (await admin.GetFromJsonAsync<UserPageResponse>("/api/auth/admin/users?search=intern&role=aplicador&page=1&pageSize=1"))!;
        Assert.Equal(1, list.Total); Assert.Single(list.Items); Assert.Equal(intern.Id, list.Items[0].Id);
        var userClient = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await Login(userClient, intern.Email, Password)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await userClient.GetAsync("/api/auth/admin/users")).StatusCode);
        var session = (await admin.GetFromJsonAsync<AuthSessionResponse>("/api/auth/session"))!;
        var me = (await admin.GetFromJsonAsync<UserResponse>($"/api/auth/admin/users/{session.Id}"))!;
        var response = await Mutate(admin, HttpMethod.Put, $"/api/auth/admin/users/{me.Id}", new UpdateUserRequest(me.FullName, me.UserName, me.Email, [], false, me.ConcurrencyStamp));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("administrador ativo", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task OidcPkce_UserInfo_PasswordResetAndAuditWork()
    {
        var intern = await Create("oidcintern", [RoleCatalog.Intern]);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("http://localhost:8081"), AllowAutoRedirect = false });
        await Login(client, intern.UserName, Password);
        var discovery = await client.GetFromJsonAsync<JsonElement>("/.well-known/openid-configuration");
        Assert.Equal("http://localhost:8081/", discovery.GetProperty("issuer").GetString());
        var verifier = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var challenge = WebEncoders.Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
        var authorize = QueryHelpers.AddQueryString("/connect/authorize", new Dictionary<string, string?>
        {
            ["client_id"] = "vet-control", ["redirect_uri"] = "http://localhost:8080/signin-oidc",
            ["response_type"] = "code", ["scope"] = "openid profile email roles offline_access vet-control-api",
            ["code_challenge"] = challenge, ["code_challenge_method"] = "S256", ["state"] = "test-state", ["nonce"] = "test-nonce"
        });
        var redirect = await client.GetAsync(authorize);
        Assert.Equal(HttpStatusCode.Redirect, redirect.StatusCode);
        var code = QueryHelpers.ParseQuery(redirect.Headers.Location!.Query)["code"].ToString();
        Assert.NotEmpty(code);
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code", ["code"] = code, ["code_verifier"] = verifier,
            ["client_id"] = "vet-control", ["client_secret"] = "vet-control-development-secret-change-in-production",
            ["redirect_uri"] = "http://localhost:8080/signin-oidc"
        };
        var token = await client.PostAsync("/connect/token", new FormUrlEncodedContent(form));
        Assert.True(token.IsSuccessStatusCode, await token.Content.ReadAsStringAsync());
        var tokens = await token.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = tokens.GetProperty("access_token").GetString();
        var refreshToken = tokens.GetProperty("refresh_token").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var info = await client.GetFromJsonAsync<JsonElement>("/connect/userinfo");
        Assert.Equal(intern.Id.ToString(), info.GetProperty("sub").GetString());
        Assert.Contains(RoleCatalog.Handler, info.GetProperty("role").EnumerateArray().Select(x => x.GetString()));
        intern = (await admin.GetFromJsonAsync<UserResponse>($"/api/auth/admin/users/{intern.Id}"))!;
        var reset = await Mutate(admin, HttpMethod.Post, $"/api/auth/admin/users/{intern.Id}/reset-password", new ResetPasswordRequest("Temporary-Test2!", intern.ConcurrencyStamp));
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/connect/userinfo")).StatusCode);
        var refresh = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token", ["refresh_token"] = refreshToken,
            ["client_id"] = form["client_id"], ["client_secret"] = form["client_secret"]
        }));
        Assert.Equal(HttpStatusCode.BadRequest, refresh.StatusCode);
        var replay = await client.PostAsync("/connect/token", new FormUrlEncodedContent(form));
        Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
        var history = (await admin.GetFromJsonAsync<AuditPageResponse>($"/api/auth/admin/users/{intern.Id}/audit"))!;
        Assert.Contains(history.Items, x => x.Action == "user.password_reset");
        Assert.DoesNotContain(Password, JsonSerializer.Serialize(history));
        Assert.DoesNotContain("PasswordHash", JsonSerializer.Serialize(history));
        Assert.DoesNotContain("ConcurrencyStamp", JsonSerializer.Serialize(history));
    }

    [Fact]
    public async Task SessionCookieOfVetApiIsCreatedByOidcAndRevokedAfterDisable()
    {
        var user = await Create("vetuser", [RoleCatalog.Applicator]);
        var providerClient = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("http://localhost:8081"), AllowAutoRedirect = false });
        await Login(providerClient, user.UserName, Password);
        await using var api = new VetFactory(factory);
        var client = api.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("http://localhost:8080"), AllowAutoRedirect = false });
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/animais/count")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/v1/validacao-vacinacao/not-a-guid")).StatusCode);
        Assert.Contains(api.Services.GetRequiredService<EndpointDataSource>().Endpoints,
            endpoint => endpoint is RouteEndpoint route && route.RoutePattern.RawText!.Contains("validacao-vacinacao") &&
                endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null);
        var login = await client.GetAsync("/api/v1/auth/login?returnUrl=/prontuario/test");
        Assert.True(login.StatusCode == HttpStatusCode.Redirect, await login.Content.ReadAsStringAsync());
        var authorization = await providerClient.GetAsync(login.Headers.Location);
        Assert.Equal(HttpStatusCode.Redirect, authorization.StatusCode);
        var callback = await client.GetAsync(authorization.Headers.Location);
        Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
        Assert.True(callback.Headers.Location?.ToString() == "http://localhost:5173/prontuario/test",
            callback.Headers.TryGetValues("Test-AuthError", out var failures) ? string.Join("; ", failures) : callback.Headers.Location?.ToString());
        var session = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/session");
        Assert.True(session.GetProperty("authenticated").GetBoolean());
        Assert.Contains(RoleCatalog.Applicator, session.GetProperty("roles").EnumerateArray().Select(x => x.GetString()));
        var cache = (MemoryCache)api.Services.GetRequiredService<IMemoryCache>();
        var key = cache.Keys.OfType<string>().Single(x => x.StartsWith("vet-session:", StringComparison.Ordinal));
        var tickets = api.Services.GetRequiredService<VetApi::Api.Security.ServerTicketStore>();
        tickets.Find(key)!.Ticket.Properties.UpdateTokenValue("expires_at", DateTimeOffset.UtcNow.AddMinutes(-1).ToString("o"));
        var concurrent = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => client.GetFromJsonAsync<JsonElement>("/api/v1/auth/session")));
        Assert.All(concurrent, item => Assert.True(item.GetProperty("authenticated").GetBoolean()));
        Assert.True(DateTimeOffset.Parse(tickets.Find(key)!.Ticket.Properties.GetTokenValue("expires_at")!) > DateTimeOffset.UtcNow);
        user = (await admin.GetFromJsonAsync<UserResponse>($"/api/auth/admin/users/{user.Id}"))!;
        var disabled = await Mutate(admin, HttpMethod.Put, $"/api/auth/admin/users/{user.Id}",
            new UpdateUserRequest(user.FullName, user.UserName, user.Email, user.Roles, false, user.ConcurrencyStamp));
        Assert.Equal(HttpStatusCode.OK, disabled.StatusCode);
        session = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/session");
        Assert.False(session.GetProperty("authenticated").GetBoolean());
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/v1/validacao-vacinacao/not-a-guid")).StatusCode);
    }

    [Fact]
    public async Task ClinicLogoutRejectsMissingOrInvalidCsrfWithoutEndingSession()
    {
        await using var api = new VetFactory(factory);
        using var client = await LoginToClinic(api);
        var missing = await client.PostAsync("/api/v1/auth/logout", new FormUrlEncodedContent(new Dictionary<string, string>()));
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        var invalid = await client.PostAsync("/api/v1/auth/logout", new FormUrlEncodedContent(new Dictionary<string, string>
        { ["__RequestVerificationToken"] = "invalid" }));
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.True((await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/session")).GetProperty("authenticated").GetBoolean());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BrowserLogoutEndsBothAppsThroughOidc(bool fromAccountPanel)
    {
        await using var api = new VetFactory(factory);
        using var client = await LoginToClinic(api);
        using var otherDevice = await LoginToClinic(api);
        var session = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/session");
        var token = session.GetProperty("csrfToken").GetString()!;
        var store = api.Services.GetRequiredService<VetApi::Api.Security.ServerTicketStore>();
        var cache = (MemoryCache)api.Services.GetRequiredService<IMemoryCache>();
        var sessionsBefore = cache.Keys.OfType<string>().Where(x => x.StartsWith("vet-session:", StringComparison.Ordinal)).ToArray();
        Assert.Equal(2, sessionsBefore.Length);

        if (fromAccountPanel)
        {
            // The account-panel button first ends Identity, then navigates via the clinic logout form.
            Assert.Equal(HttpStatusCode.NoContent, (await Mutate(admin, HttpMethod.Post, "/api/auth/logout", new { })).StatusCode);
            Assert.False((await admin.GetFromJsonAsync<AuthSessionResponse>("/api/auth/session"))!.Authenticated);
        }
        var logout = await client.PostAsync("/api/v1/auth/logout", new FormUrlEncodedContent(new Dictionary<string, string>
        { ["__RequestVerificationToken"] = token }));
        Assert.True(logout.StatusCode == HttpStatusCode.Redirect, await logout.Content.ReadAsStringAsync());
        Assert.Equal("/connect/logout", logout.Headers.Location!.AbsolutePath);
        var parameters = QueryHelpers.ParseQuery(logout.Headers.Location.Query);
        Assert.Equal("http://localhost:8080/signout-callback-oidc", parameters["post_logout_redirect_uri"].ToString());
        Assert.NotEmpty(parameters["id_token_hint"].ToString());
        var providerLogout = await admin.GetAsync(logout.Headers.Location);
        Assert.True(providerLogout.StatusCode == HttpStatusCode.Redirect, await providerLogout.Content.ReadAsStringAsync());
        var callback = await client.GetAsync(providerLogout.Headers.Location);
        Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
        Assert.Equal("http://localhost:5173/login", callback.Headers.Location!.ToString());
        Assert.False((await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/session")).GetProperty("authenticated").GetBoolean());
        Assert.False((await admin.GetFromJsonAsync<AuthSessionResponse>("/api/auth/session"))!.Authenticated);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/animais/count")).StatusCode);
        Assert.Single(sessionsBefore, key => store.Find(key) is not null);
        Assert.True((await otherDevice.GetFromJsonAsync<JsonElement>("/api/v1/auth/session")).GetProperty("authenticated").GetBoolean());
    }

    private async Task<HttpClient> LoginToClinic(VetFactory api)
    {
        var client = api.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("http://localhost:8080"), AllowAutoRedirect = false });
        var login = await client.GetAsync("/api/v1/auth/login");
        var authorization = await admin.GetAsync(login.Headers.Location);
        var callback = await client.GetAsync(authorization.Headers.Location);
        Assert.True(callback.Headers.Location?.ToString() == "http://localhost:5173/",
            callback.Headers.TryGetValues("Test-AuthError", out var failures) ? string.Join("; ", failures) : callback.Headers.Location?.ToString());
        return client;
    }

    [Fact]
    public async Task RequiredPasswordChangeBlocksAdminAndPreservesSafeRedirects()
    {
        var user = await Create("forcedadmin", [RoleCatalog.Admin], mustChange: true);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("http://localhost:8081"), AllowAutoRedirect = false });
        var login = await Mutate(client, HttpMethod.Post, "/api/auth/login", new LoginRequest(user.UserName, Password, "https://outside.test/"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var result = (await login.Content.ReadFromJsonAsync<LoginResponse>())!;
        Assert.True(result.MustChangePassword); Assert.Equal("http://localhost:5173/auth", result.NextUrl);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/auth/admin/users")).StatusCode);
        var change = await Mutate(client, HttpMethod.Post, "/api/auth/password", new PasswordChangeRequest(Password, "My-New-Password2!"));
        Assert.Equal(HttpStatusCode.NoContent, change.StatusCode);
        var session = (await client.GetFromJsonAsync<AuthSessionResponse>("/api/auth/session"))!;
        Assert.False(session.MustChangePassword);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/admin/users")).StatusCode);
    }

    [Fact]
    public async Task AuthContractGeneratorUsesItsOwnRunningApiAndPreservesClinicContract()
    {
        var root = Path.GetFullPath(Path.Combine(factory.Services.GetRequiredService<IWebHostEnvironment>().ContentRootPath, ".."));
        var clinicFile = Path.Combine(root, "web", "src", "lib", "types", "contract.d.ts");
        var before = await File.ReadAllBytesAsync(clinicFile);
        await using var live = new AuthFactory(connection);
        live.UseKestrel(0);
        using var client = live.CreateClient();
        var output = Environment.GetEnvironmentVariable("AUTH_GENERATE_PROJECT_CONTRACT") == "1"
            ? Path.Combine(root, "web", "src", "lib", "types", "auth-contract.d.ts")
            : Path.Combine(root, "artifacts", "auth-contract-test.d.ts");
        var start = new ProcessStartInfo("node", "scripts/generate-auth-contract.mjs")
        { WorkingDirectory = Path.Combine(root, "web"), RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        start.Environment["AUTH_API_URL"] = client.BaseAddress!.ToString();
        start.Environment["AUTH_CONTRACT_OUTPUT"] = output;
        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        Assert.True(process.ExitCode == 0, await stdout + await stderr);
        var declarations = await File.ReadAllTextAsync(output);
        Assert.Contains("/api/auth/session", declarations);
        Assert.Contains("UserResponse", declarations);
        Assert.Contains("AuditPageResponse", declarations);
        Assert.DoesNotContain("/api/v1/animais", declarations);
        Assert.Equal(before, await File.ReadAllBytesAsync(clinicFile));
    }

    private sealed class AuthFactory(string connection) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<AuthContext>>(); services.RemoveAll<AuthContext>();
                services.RemoveAll<Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<AuthContext>>();
                services.AddDbContext<AuthContext>(options => options.UseNpgsql(connection).UseOpenIddict());
            });
        }
    }
    private sealed class VetFactory(AuthFactory provider) : WebApplicationFactory<VetApi::Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.Backchannel = provider.CreateClient();
                    var failureHandler = options.Events.OnRemoteFailure;
                    options.Events.OnRemoteFailure = context =>
                    {
                        context.Response.Headers["Test-AuthError"] = context.Failure?.Message;
                        return failureHandler(context);
                    };
                    options.ConfigurationManager = new Microsoft.IdentityModel.Protocols.ConfigurationManager<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration>(
                        options.MetadataAddress, new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfigurationRetriever(),
                        new Microsoft.IdentityModel.Protocols.HttpDocumentRetriever(options.Backchannel) { RequireHttps = false });
                });
                services.AddHttpClient("identity").ConfigurePrimaryHttpMessageHandler(() => provider.Server.CreateHandler());
            });
        }
    }
}
