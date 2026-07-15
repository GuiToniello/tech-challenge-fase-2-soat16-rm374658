using Resend;

namespace TechChallenge.Oficina.Email.Features.Orcamentos;

public interface IResendClient
{
    Task SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
}
