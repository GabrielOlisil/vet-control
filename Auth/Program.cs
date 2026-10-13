using System.Threading.RateLimiting;
using Auth.Database;
using Auth.Endpoints;
using Auth.Models;
using Auth.Security;
using Auth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AuthContext>(options => options.UseNpgsql(AuthContext.ConnectionString).UseOpenIddict());
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserAdministration>();
builder.Services.AddScoped<InitialSetup>();
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 10;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
}).AddEntityFrameworkStores<AuthContext>().AddDefaultTokenProviders().AddClaimsPrincipalFactory<UserClaimsFactory>();
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "VetControl.Identity";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
    var original = options.Events.OnValidatePrincipal;
    options.Events.OnValidatePrincipal = async context =>
    {
        await original(context);
        if (context.Principal?.Identity?.IsAuthenticated != true) return;
        var user = await context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>().GetUserAsync(context.Principal!);
        if (user is null || !user.IsActive || await context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>().IsLockedOutAsync(user))
        { context.RejectPrincipal(); await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme); }
    };
});
builder.Services.AddAuthorization(options => options.AddPolicy("UserAdministration", policy =>
    policy.RequireAuthenticatedUser().RequireRole(RoleCatalog.Admin).RequireAssertion(context =>
        !context.User.HasClaim("must_change_password", "true"))));
builder.Services.AddAntiforgery(options => { options.HeaderName = "X-CSRF-TOKEN"; options.Cookie.Name = "VetControl.Identity.Csrf"; });
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins(builder.Configuration["Frontend:Origin"]!)
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddOpenIddict().AddCore(options => options.UseEntityFrameworkCore().UseDbContext<AuthContext>())
    .AddServer(options =>
    {
        options.SetIssuer(new Uri(builder.Configuration["OpenId:Issuer"]!));
        options.SetAuthorizationEndpointUris("/connect/authorize").SetTokenEndpointUris("/connect/token")
            .SetUserInfoEndpointUris("/connect/userinfo").SetEndSessionEndpointUris("/connect/logout");
        options.AllowAuthorizationCodeFlow().AllowRefreshTokenFlow().RequireProofKeyForCodeExchange();
        options.RegisterScopes(Scopes.Profile, Scopes.Email, Scopes.Roles, "vet-control-api");
        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(15));
        options.SetRefreshTokenLifetime(TimeSpan.FromHours(8));
        if (builder.Environment.IsDevelopment()) options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
        else
        {
            options.AddEncryptionCertificate(System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12FromFile(
                builder.Configuration["OpenId:EncryptionCertificatePath"]!, builder.Configuration["OpenId:EncryptionCertificatePassword"]));
            options.AddSigningCertificate(System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12FromFile(
                builder.Configuration["OpenId:SigningCertificatePath"]!, builder.Configuration["OpenId:SigningCertificatePassword"]));
        }
        var integration = options.UseAspNetCore().EnableAuthorizationEndpointPassthrough().EnableTokenEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough().EnableEndSessionEndpointPassthrough();
        if (builder.Environment.IsDevelopment()) integration.DisableTransportSecurityRequirement();
    }).AddValidation(options =>
    {
        options.UseLocalServer(); options.UseAspNetCore();
        options.EnableTokenEntryValidation(); options.EnableAuthorizationEntryValidation();
    });
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict);
builder.Services.AddOpenApi();
var app = builder.Build();
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    var status = error is UserOperationException operation ? operation.StatusCode :
        error is DbUpdateConcurrencyException ? 409 :
        error is Npgsql.PostgresException { SqlState: "40001" } ? 409 : 500;
    await Results.Problem(statusCode: status, detail: status == 500 ? "Não foi possível concluir a operação." : error?.Message).ExecuteAsync(context);
}));
app.UseCors(); app.UseRateLimiter(); app.UseAuthentication(); app.UseAuthorization();
app.Use(async (context, next) => { context.Response.Headers.CacheControl = "no-store"; await next(); });
if (app.Environment.IsDevelopment()) app.MapOpenApi().AllowAnonymous();
app.MapAccountEndpoints(); app.MapAdminEndpoints(); app.MapOpenIdEndpoints();
await Bootstrap.Initialize(app.Services, app.Configuration);
app.Run();

public partial class Program;
