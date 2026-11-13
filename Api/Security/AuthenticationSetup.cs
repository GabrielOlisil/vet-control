using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

namespace Api.Security;

public static class AuthenticationSetup
{
    public static void AddVetAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(sp =>
        {
            var provider = sp.GetRequiredService<IDataProtectionProvider>();
            var protector = provider.CreateProtector(
                "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
                CookieAuthenticationDefaults.AuthenticationScheme,
                "v2");
            return new TicketDataFormat(protector);
        });
        builder.Services.AddSingleton<ServerTicketStore>();
        builder.Services.AddScoped<VetSessionValidator>();
        var identityClient = builder.Services.AddHttpClient("identity", client => client.Timeout = TimeSpan.FromSeconds(15));
        var backchannelUrl = builder.Configuration["Authentication:BackchannelBaseUrl"];
        Func<HttpMessageHandler>? createBackchannel = null;
        if (!string.IsNullOrWhiteSpace(backchannelUrl))
        {
            var authority = new Uri(builder.Configuration["Authentication:Authority"]!, UriKind.Absolute);
            var destination = new Uri(backchannelUrl, UriKind.Absolute);
            createBackchannel = () => new AuthorityBackchannelHandler(authority, destination) { InnerHandler = new HttpClientHandler() };
            identityClient.ConfigurePrimaryHttpMessageHandler(createBackchannel);
        }
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }).AddCookie(options =>
        {
            options.Cookie.Name = "VetControl.Session"; options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromHours(8); options.SlidingExpiration = false;
            options.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = 401; return Task.CompletedTask; };
            options.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = 403; return Task.CompletedTask; };
            options.Events.OnValidatePrincipal = ctx =>
            {
                // Anonymous documents and login/discovery must also work when Identity is unavailable.
                if (ctx.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null &&
                    ctx.HttpContext.Request.Path != "/api/v1/auth/session")
                { ctx.RejectPrincipal(); return Task.CompletedTask; }
                return ctx.HttpContext.RequestServices.GetRequiredService<VetSessionValidator>().Validate(ctx);
            };
        }).AddOpenIdConnect(options =>
        {
            options.Authority = builder.Configuration["Authentication:Authority"]!;
            if (createBackchannel is not null) options.BackchannelHttpHandler = createBackchannel();
            options.ClientId = builder.Configuration["Authentication:ClientId"]!;
            options.ClientSecret = builder.Configuration["Authentication:ClientSecret"]!;
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            options.ResponseType = "code"; options.ResponseMode = "query"; options.UsePkce = true; options.SaveTokens = true;
            options.MapInboundClaims = false;
            options.Scope.Clear();
            foreach (var scope in new[] { "openid", "profile", "email", "roles", "offline_access", "vet-control-api" }) options.Scope.Add(scope);
            options.TokenValidationParameters.NameClaimType = "name";
            options.TokenValidationParameters.RoleClaimType = "role";
            options.GetClaimsFromUserInfoEndpoint = true;
            if (builder.Environment.IsDevelopment())
            {
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.NonceCookie.SameSite = SameSiteMode.Lax;
                options.NonceCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            }
            options.ClaimActions.MapUniqueJsonKey("role", "role");
            options.ClaimActions.MapUniqueJsonKey("preferred_username", "preferred_username");
            options.Events.OnRemoteFailure = context =>
            {
                context.HandleResponse();
                context.Response.Redirect(builder.Configuration["Authentication:FrontendOrigin"] + "/login?erro=autenticacao");
                return Task.CompletedTask;
            };
        });
        builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
            .Configure<ServerTicketStore>((options, store) => options.SessionStore = store);
        builder.Services.AddAntiforgery(options => { options.HeaderName = "X-CSRF-TOKEN"; options.Cookie.Name = "VetControl.Session.Csrf"; });
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                .RequireRole("admin", "medico_veterinario", "manejador_animal", "aplicador", "estagiario_medvet").Build();

            AddPolicy(options, ClinicalPolicies.ManageAnimals, "admin", "medico_veterinario", "manejador_animal", "estagiario_medvet");
            AddPolicy(options, ClinicalPolicies.ManageSpeciesAndBreeds, "admin", "medico_veterinario", "manejador_animal", "estagiario_medvet");
            AddPolicy(options, ClinicalPolicies.ManageVaccines, "admin", "medico_veterinario", "estagiario_medvet");
            AddPolicy(options, ClinicalPolicies.ManageProtocols, "admin", "medico_veterinario");
            AddPolicy(options, ClinicalPolicies.ManageMovements, "admin", "medico_veterinario", "manejador_animal", "estagiario_medvet");
            AddPolicy(options, ClinicalPolicies.ManageVaccineApplications, "admin", "medico_veterinario", "aplicador", "estagiario_medvet");
            AddPolicy(options, ClinicalPolicies.DeleteVaccineApplications, "admin", "medico_veterinario");
            AddPolicy(options, ClinicalPolicies.SignVaccineApplications, "medico_veterinario");
        });
    }

    private static void AddPolicy(AuthorizationOptions options, string name, params string[] roles)
    {
        options.AddPolicy(name, policy => policy.RequireAuthenticatedUser().RequireRole(roles));
    }
}
