using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Oficina.Entities.Features.Orcamentos;
using TechChallenge.Oficina.Email.Configuration;
using TechChallenge.Oficina.Email.Features.Orcamentos;

namespace TechChallenge.Oficina.Email;

public static class Extensions
{
    public static IServiceCollection AddInfraEmail(this IServiceCollection services, ResendSettings resendSettings)
    {
        services.AddSingleton(resendSettings);
        services.AddSingleton<Resend.IResend>(_ => Resend.ResendClient.Create(
            string.IsNullOrWhiteSpace(resendSettings.ApiKey) ? "dummy" : resendSettings.ApiKey));
        services.AddScoped<IResendClient, ResendClientAdapter>();
        services.AddScoped<IOrcamentoEmailSender, OrcamentoEmailSender>();

        return services;
    }
}
