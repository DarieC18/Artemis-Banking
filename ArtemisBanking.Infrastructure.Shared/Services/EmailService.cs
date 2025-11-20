using ArtemisBanking.Application.DTOs.Email;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Infrastructure.Shared.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text;

namespace ArtemisBanking.Infrastructure.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailRequestDto emailRequestDto)
        {
            try
            {
                emailRequestDto.ToRange ??= new List<string>();

                if (!string.IsNullOrWhiteSpace(emailRequestDto.To))
                {
                    emailRequestDto.ToRange.Add(emailRequestDto.To);
                }

                var recipients = emailRequestDto.ToRange
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Select(r => r.Trim())
                    .ToList();

                _logger.LogInformation("Preparando envío de correo a: {Recipients}", string.Join(", ", recipients));

                if (string.IsNullOrWhiteSpace(_mailSettings.EmailFrom))
                {
                    throw new InvalidOperationException("La configuracion de correo no especifica EmailFrom");
                }

                if (string.IsNullOrWhiteSpace(_mailSettings.SmtpHost))
                {
                    throw new InvalidOperationException("La configuracion de correo no especifica SmtpHost");
                }

                var email = new MimeMessage
                {
                    Sender = MailboxAddress.Parse(_mailSettings.EmailFrom),
                    Subject = emailRequestDto.Subject ?? "(sin asunto)"
                };

                if (!string.IsNullOrWhiteSpace(_mailSettings.DisplayName) && !string.IsNullOrWhiteSpace(_mailSettings.EmailFrom))
                {
                    email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.EmailFrom));
                }
                else if (!string.IsNullOrWhiteSpace(_mailSettings.EmailFrom))
                {
                    email.From.Add(MailboxAddress.Parse(_mailSettings.EmailFrom));
                }

                foreach (var recipient in recipients)
                {
                    email.To.Add(MailboxAddress.Parse(recipient));
                }

                var builder = new BodyBuilder
                {
                    HtmlBody = emailRequestDto.Body,
                    TextBody = emailRequestDto.Body
                };

                email.Body = builder.ToMessageBody();

                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(
                    _mailSettings.SmtpHost,
                    _mailSettings.SmtpPort,
                    SecureSocketOptions.StartTls);

                if (!string.IsNullOrWhiteSpace(_mailSettings.SmtpUser) && !string.IsNullOrWhiteSpace(_mailSettings.SmtpPass))
                {
                    await smtpClient.AuthenticateAsync(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                }

                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);

                _logger.LogInformation("Correo enviado correctamente a: {Recipients}", string.Join(", ", recipients));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo: {Message}", ex.Message);
                throw;
            }
        }

        public Task SendAccountConfirmationEmailAsync(string email, string userName, string token)
        {
            var subject = "Confirma tu cuenta - ArtemisBanking";
            var confirmationLink = GenerateConfirmationLink(token);
            var body = new StringBuilder()
                .AppendLine($"Hola {userName}!")
                .AppendLine("Gracias por registrarte en ArtemisBanking.")
                .AppendLine("Para activar tu cuenta, haz clic en el siguiente enlace:")
                .AppendLine($"<a href=\"{confirmationLink}\">Activar cuenta</a>")
                .AppendLine()
                .AppendLine("Si tú no solicitaste este registro, puedes ignorar este correo.")
                .ToString();

            return SendAsync(new EmailRequestDto
            {
                To = email,
                Subject = subject,
                Body = body.ToString()
            });
        }

        public Task SendPasswordResetEmailAsync(string email, string userName, string token)
        {
            var subject = "Restablece tu contraseña - ArtemisBanking";
            var body = new StringBuilder()
                .AppendLine($"Hola {userName}!")
                .AppendLine("Recibimos una solicitud para restablecer la contraseña")
                .AppendLine("Para continuar, haz clic en el siguiente enlace:")
                .AppendLine(GenerateResetPasswordLink(token, userName))
                .AppendLine()
                .AppendLine("Si tú no solicitaste este restablecimiento, puedes ignorar este correo")
                .ToString();

            return SendAsync(new EmailRequestDto
            {
                To = email,
                Subject = subject,
                Body = body.ToString()
            });
        }

        private string GenerateConfirmationLink(string token)
        {
            var baseUrl = _mailSettings.WebAppBaseUrl?.TrimEnd('/')
                          ?? _mailSettings.ClientAppUrl?.TrimEnd('/')
                          ?? "http://localhost:5221";
            return $"{baseUrl}/Account/Activate?token={Uri.EscapeDataString(token)}";
        }

        private string GenerateResetPasswordLink(string token, string userName)
        {
            var baseUrl = _mailSettings.ClientAppUrl?.TrimEnd('/') ?? "https://localhost:5001";
            return $"{baseUrl}/Account/ResetPassword?token={Uri.EscapeDataString(token)}&userName={Uri.EscapeDataString(userName)}";
        }
        public Task SendAccountConfirmationTokenPlainAsync(string email, string userName, string token)
        {
            var subject = "Token de activación de tu cuenta - ArtemisBanking";

            var body = new StringBuilder()
                .AppendLine($"Hola {userName}!")
                .AppendLine()
                .AppendLine("Tu cuenta en ArtemisBanking ha sido creada correctamente.")
                .AppendLine("Para activarla, copia el siguiente token y pegalo en la pantalla de activacin de la app:")
                .AppendLine()
                .AppendLine("TOKEN DE ACTIVACIÓN:")
                .AppendLine(token)
                .AppendLine()
                .AppendLine("Si tu no solicitaste esta cuenta, puedes ignorar este correo.")
                .ToString();

            return SendAsync(new EmailRequestDto
            {
                To = email,
                Subject = subject,
                Body = body
            });
        }

    }
}
