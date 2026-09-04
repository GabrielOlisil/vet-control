using Api.Database;
using Api.Middlewares;
using Api.Services.AplicacoesVacina;
using Api.Services.Animals;
using Api.Services.CartoesVacina;
using Api.Services.Especies;
using Api.Services.Racas;
using Api.Services.Vacinas;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<ApiContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("ApiContext") ??
                  throw new InvalidOperationException("Connection string 'ApiContext' not found.")));

builder.Services.AddScoped<IEspecieService, EspecieService>();
builder.Services.AddScoped<IRacaService, RacaService>();
builder.Services.AddScoped<IAnimalService, AnimalService>();
builder.Services.AddScoped<IVacinaService, VacinaService>();
builder.Services.AddScoped<IAplicacaoVacinaService, AplicacaoVacinaService>();
builder.Services.AddScoped<ICartaoVacinaService, CartaoVacinaService>();


builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<ErrorHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApiContext>();
    dbContext.Database.Migrate();
}


app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("docs");
}


app.MapControllers();
app.Run();

