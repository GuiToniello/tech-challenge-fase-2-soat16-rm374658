using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TechChallenge.Oficina.Application;
using TechChallenge.Oficina.API.Extensions;
using TechChallenge.Oficina.API.Middleware;
using TechChallenge.Oficina.Infra.Configuration;
using TechChallenge.Oficina.Infra.Data;
using TechChallenge.Oficina.Infra.Email;
using TechChallenge.Oficina.Infra.Email.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(modelState => modelState.Value?.Errors.Count > 0)
                .SelectMany(modelState => modelState.Value!.Errors)
                .Select(error => error.ErrorMessage)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .ToList();

            var firstMessage = errors.FirstOrDefault() ?? "Dados de entrada invalidos.";

            return new BadRequestObjectResult(new
            {
                message = firstMessage,
                errors
            });
        };
    });
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

var app = builder.Build();

app.Services.ApplyMigrations();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapControllers();
await app.RunAsync();
