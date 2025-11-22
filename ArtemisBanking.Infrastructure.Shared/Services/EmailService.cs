using ArtemisBanking.Application.DTOs.Email;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Infrastructure.Shared.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

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

            var body = $@"
            <html>
              <body style=""font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; padding: 20px;"">
                <table width=""100%"" cellpadding=""0"" cellspacing=""0""
                       style=""max-width: 600px; margin: auto; background: #ffffff; border-radius: 10px;
                              padding: 25px; border: 1px solid #e0e0e0;"">
                  <tr>
                    <td>

                      <h2 style=""color: #2e2e2e; margin-bottom: 15px;"">
                        Confirma tu cuenta
                      </h2>

                      <p style=""font-size: 15px; margin-bottom: 10px;"">
                        Hola <strong>{userName}</strong>!
                      </p>

                      <p style=""font-size: 15px; line-height: 1.5;"">
                        Gracias por registrarte en <strong>ArtemisBanking</strong>.
                      </p>

                      <p style=""font-size: 15px; line-height: 1.5;"">
                        Para activar tu cuenta, haz clic en el siguiente enlace:
                      </p>

                      <p style=""margin-top: 20px; font-size: 15px;"">
                        <a href=""{confirmationLink}"" 
                           style=""color: #1a73e8; text-decoration: none; font-weight: bold;"">
                          Activar cuenta
                        </a>
                      </p>

                      <div style=""margin-top: 25px; padding: 15px; background-color: #fff4e5;
                                  border-left: 4px solid #ffa726; font-size: 14px;"">
                        Si tú no solicitaste este registro, puedes ignorar este correo.
                      </div>

                      <p style=""margin-top: 30px; color: #888; font-size: 12px;"">
                        ArtemisBanking © {DateTime.Now.Year}
                      </p>

                    </td>
                  </tr>
                </table>
              </body>
            </html>
            ";

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
            var resetLink = GenerateResetPasswordLink(token, userName);
            var body = $@"
            <html>
              <body style=""font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; padding: 20px;"">
                <table width=""100%"" cellpadding=""0"" cellspacing=""0""
                       style=""max-width: 600px; margin: auto; background: #ffffff; border-radius: 10px;
                              padding: 25px; border: 1px solid #e0e0e0;"">
                  <tr>
                    <td>

                      <h2 style=""color: #2e2e2e; margin-bottom: 15px;"">
                        Restablecimiento de contraseña
                      </h2>

                      <p style=""font-size: 15px; margin-bottom: 10px;"">
                        Hola <strong>{userName}</strong>!
                      </p>

                      <p style=""font-size: 15px; line-height: 1.5;"">
                        Hemos recibido una solicitud para restablecer tu contraseña.
                      </p>

                      <p style=""font-size: 15px; line-height: 1.5;"">
                        Para continuar, haz clic en el siguiente enlace:
                      </p>

                      <p style=""margin-top: 20px; font-size: 15px;"">
                        <a href=""{resetLink}""
                           style=""color: #1a73e8; text-decoration: none; font-weight: bold;"">
                          Restablecer contraseña
                        </a>
                      </p>

                      <div style=""margin-top: 25px; padding: 15px; background-color: #fff4e5;
                                  border-left: 4px solid #ffa726; font-size: 14px;"">
                        Si tú no solicitaste este restablecimiento, puedes ignorar este correo.
                      </div>

                      <p style=""margin-top: 30px; color: #888; font-size: 12px;"">
                        ArtemisBanking © {DateTime.Now.Year}
                      </p>

                    </td>
                  </tr>
                </table>
              </body>
            </html>
            ";

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
            var body = $@"
            <html>
              <body style=""font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; padding: 20px;"">
                <table width=""100%"" cellpadding=""0"" cellspacing=""0""
                       style=""max-width: 600px; margin: auto; background: #ffffff; border-radius: 10px;
                              padding: 25px; border: 1px solid #e0e0e0;"">
                  <tr>
                    <td>

                      <h2 style=""color: #2e2e2e; margin-bottom: 15px;"">
                        Activación de cuenta
                      </h2>

                      <p style=""font-size: 15px; margin-bottom: 10px;"">
                        Hola <strong>{userName}</strong>!
                      </p>

                      <p style=""font-size: 15px; line-height: 1.5;"">
                        Tu cuenta en <strong>ArtemisBanking</strong> ha sido creada correctamente.
                      </p>

                      <p style=""font-size: 15px; line-height: 1.5;"">
                        Para activarla, copia el siguiente token y pégalo en la pantalla de activación de la app:
                      </p>

                      <div style=""margin-top: 20px; padding: 18px; background-color: #f4f4f4; 
                                  border: 1px solid #ddd; border-radius: 6px; font-size: 16px; 
                                  font-weight: bold; letter-spacing: 1px; text-align: center;"">
                        {token}
                      </div>

                      <div style=""margin-top: 25px; padding: 15px; background-color: #fff4e5;
                                  border-left: 4px solid #ffa726; font-size: 14px;"">
                        Si tú no solicitaste esta cuenta, puedes ignorar este correo.
                      </div>

                      <p style=""margin-top: 30px; color: #888; font-size: 12px;"">
                        ArtemisBanking © {DateTime.Now.Year}
                      </p>

                    </td>
                  </tr>
                </table>
              </body>
            </html>
            ";

            return SendAsync(new EmailRequestDto
            {
                To = email,
                Subject = subject,
                Body = body
            });
        }

    }
}
