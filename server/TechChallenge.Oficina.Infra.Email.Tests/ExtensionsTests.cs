using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Oficina.Domain.Features.Orcamentos;
using TechChallenge.Oficina.Infra.Email;
using TechChallenge.Oficina.Infra.Email.Configuration;
using TechChallenge.Oficina.Infra.Email.Features.Orcamentos;
using Xunit;

namespace TechChallenge.Oficina.Infra.Email.Tests;

public sealed class ExtensionsTests
{
    [Fact]
    public void AddInfraEmail_DeveRegistrarServicos()
    {
        var services = new ServiceCollection();
        var settings = new ResendSettings { ApiKey = "api-key", FromEmail = "onboarding@resend.dev" };

        services.AddInfraEmail(settings);

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IOrcamentoEmailSender>());
        Assert.NotNull(provider.GetService<IResendClient>());
    }

    [Fact]
    public void AddInfraEmail_DeveRegistrarServicosQuandoConfiguracoesVazias()
    {
        var services = new ServiceCollection();
        var settings = new ResendSettings { ApiKey = string.Empty, FromEmail = string.Empty };

        services.AddInfraEmail(settings);

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IOrcamentoEmailSender>());
        Assert.NotNull(provider.GetService<IResendClient>());
    }
}
