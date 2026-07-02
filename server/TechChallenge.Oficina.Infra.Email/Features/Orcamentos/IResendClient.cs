using Resend;

namespace TechChallenge.Oficina.Infra.Email.Features.Orcamentos;

public interface IResendClient
{
    Task SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
}
