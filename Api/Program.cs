using Api.Database;
using Api.Middlewares;
using Api.Security;
using Api.Services.AplicacoesVacina;
using Api.Services.Animals;
using Api.Services.Especies;
using Api.Services.MovimentacoesAnimais;
using Api.Services.ProtocolosVacinais;
using Api.Services.Racas;
using Api.Services.Vacinas;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using VetControl.Hosting;

const string webPolicy = "AllowWeb";

var builder = WebApplication.CreateBuilder(args);
builder.AddDeploymentDefaults("VetControl");
builder.AddVetAuthentication();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContextPool<ApiContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("ApiContext") ??
                  throw new InvalidOperationException("Connection string 'ApiContext' not found.")));

builder.Services.AddScoped<IEspecieService, EspecieService>();
builder.Services.AddScoped<IRacaService, RacaService>();
builder.Services.AddScoped<IAnimalService, AnimalService>();
builder.Services.AddScoped<IVacinaService, VacinaService>();
builder.Services.AddScoped<IAplicacaoVacinaService, AplicacaoVacinaService>();
builder.Services.AddScoped<IProtocoloVacinalService, ProtocoloVacinalService>();
builder.Services.AddScoped<IMovimentacaoAnimalService, MovimentacaoAnimalService>();
builder.Services.AddScoped<Api.Services.Ambientes.AmbienteService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<Api.Services.ClinicalCalendar>();
builder.Services.AddScoped<Api.Services.AgendaVacinal.AgendaVacinalService>();

builder.Services.AddControllers(options =>
{
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (_, field) => $"O valor informado não é válido para o campo '{field}'.");
    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(
        _ => "O valor informado não é válido.");
})
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
}
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("public-validation", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
        { PermitLimit = 60, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.OnRejected = async (context, ct) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry))
            context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retry.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture);
        await context.HttpContext.Response.WriteAsJsonAsync(new { detail = "Limite de consultas atingido. Aguarde um minuto e tente novamente." }, ct);
    };
});

builder.Services.AddExceptionHandler<ErrorHandler>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(webPolicy, policy =>
    {
        policy.WithOrigins(builder.Environment.IsDevelopment()
                ? new[] { builder.Configuration["Authentication:FrontendOrigin"]!, "http://127.0.0.1:5173" }
                : new[] { builder.Configuration["Authentication:FrontendOrigin"]! })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("X-Clinic-TimeZone")
            .AllowCredentials();
    });
});
var app = builder.Build();
app.UseDeploymentDefaults();


app.UseExceptionHandler();
app.UseStatusCodePages();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Clinic-TimeZone"] = context.RequestServices.GetRequiredService<Api.Services.ClinicalCalendar>().TimeZoneId;
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference("docs").AllowAnonymous();
}

app.UseCors(webPolicy);
app.UseRateLimiter();
// The public SPA (including its JS/CSS assets) must load before the fallback
// authorization policy protects API requests without endpoint metadata.
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true && context.Request.Path.StartsWithSegments("/api") &&
        !HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method) && !HttpMethods.IsOptions(context.Request.Method))
    {
        try { await context.RequestServices.GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>().ValidateRequestAsync(context); }
        catch (Microsoft.AspNetCore.Antiforgery.AntiforgeryValidationException)
        { context.Response.StatusCode = 400; await context.Response.WriteAsJsonAsync(new { detail = "A sessão do formulário expirou. Recarregue a página." }); return; }
    }
    await next();
});

app.MapControllers();
app.MapGet("/health/live", () => Results.Ok(new { status = "healthy" })).AllowAnonymous().ExcludeFromDescription();
app.MapGet("/health/ready", async (ApiContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct) ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503)).AllowAnonymous().ExcludeFromDescription();
app.MapFallback(async context =>
{
    var index = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "index.html");
    if (context.Request.Path.StartsWithSegments("/api") || !File.Exists(index))
    { context.Response.StatusCode = 404; return; }
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(index);
}).AllowAnonymous();


app.Run();
