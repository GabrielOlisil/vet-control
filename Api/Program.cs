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

const string webPolicy = "AllowWeb";

var builder = WebApplication.CreateBuilder(args);
builder.AddVetAuthentication();

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

builder.Services.AddControllers()
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

builder.Services.AddExceptionHandler<ErrorHandler>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(webPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
var app = builder.Build();


app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference("docs").AllowAnonymous();
}

app.UseCors(webPolicy);
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

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();


app.Run();
