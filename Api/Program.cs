using Api.Database;
using Api.Middlewares;
using Api.Services.AplicacoesVacina;
using Api.Services.Animals;
using Api.Services.Especies;
using Api.Services.Racas;
using Api.Services.Vacinas;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

const string webPolicy = "AllowWeb";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<ApiContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("ApiContext") ??
                  throw new InvalidOperationException("Connection string 'ApiContext' not found.")));

builder.Services.AddScoped<IEspecieService, EspecieService>();
builder.Services.AddScoped<IRacaService, RacaService>();
builder.Services.AddScoped<IAnimalService, AnimalService>();
builder.Services.AddScoped<IVacinaService, VacinaService>();
builder.Services.AddScoped<IAplicacaoVacinaService, AplicacaoVacinaService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<ErrorHandler>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(webPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
var app = builder.Build();


app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("docs");
}

app.UseCors(webPolicy);

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();


app.Run();