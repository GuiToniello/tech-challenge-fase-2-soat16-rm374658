using Microsoft.AspNetCore.Mvc;
using TechChallenge.Oficina.API.Extensions;
using TechChallenge.Oficina.API.Features.Clientes;
using TechChallenge.Oficina.API.Features.OrdensServico;
using TechChallenge.Oficina.API.Features.Servicos;
using TechChallenge.Oficina.API.Features.Veiculos;
using TechChallenge.Oficina.API.Middleware;
using TechChallenge.Oficina.API.Settings;
using TechChallenge.Oficina.Application;
using TechChallenge.Oficina.Infra.Data;
using TechChallenge.Oficina.Infra.Email;
using TechChallenge.Oficina.Infra.Email.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSwaggerConfiguration();
builder.Services.AddAuthConfiguration(builder.Configuration);

var databaseSettings = builder.Configuration
    .GetSection(DatabaseSettings.SectionName)
    .Get<DatabaseSettings>()
    ?? throw new InvalidOperationException("A configuração de banco de dados é obrigatória.");

if (string.IsNullOrWhiteSpace(databaseSettings.ConnectionString))
{
    throw new InvalidOperationException("A connection string do banco de dados é obrigatória.");
}

builder.Services.AddApplication();
builder.Services.AddInfraData(databaseSettings.ConnectionString);

var resendSettings = builder.Configuration
    .GetSection(ResendSettings.SectionName)
    .Get<ResendSettings>()
    ?? new ResendSettings();
builder.Services.AddInfraEmail(resendSettings);

builder.Services.RegisterClienteEndpoints();
builder.Services.RegisterVeiculoEndpoints();
builder.Services.RegisterServicoEndpoints();
builder.Services.RegisterOrdensServicoEndpoints();

var app = builder.Build();

app.Services.ApplyMigrations();


app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapClienteEndpoints();
app.MapVeiculoEndpoints();
app.MapServicoEndpoints();
app.MapOrdensServicoEndpoints();

await app.RunAsync();
