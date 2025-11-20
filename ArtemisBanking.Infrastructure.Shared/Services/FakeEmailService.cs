using ArtemisBanking.Application.DTOs.Email;
using ArtemisBanking.Application.Interfaces;

namespace ArtemisBanking.Infrastructure.Shared.Services
{
    public class FakeEmailService : IEmailService
    {
        public Task SendAsync(EmailRequestDto emailRequestDto)
        {
            Console.WriteLine($"[FAKE EMAIL] To: {emailRequestDto.To}, Subject: {emailRequestDto.Subject}");
            return Task.CompletedTask;
        }

        public Task SendAccountConfirmationEmailAsync(string email, string userName, string token)
        {
            // Simular envio de correo de confirmacion de cuenta
            Console.WriteLine($"[FAKE EMAIL] Confirmación de cuenta a {email}");
            return Task.CompletedTask;
        }

        public Task SendPasswordResetEmailAsync(string email, string userName, string token)
        {
            // Simular envio de correo de recuperacion de contraseña
            Console.WriteLine($"[FAKE EMAIL] Reset de contraseña a {email}");
            return Task.CompletedTask;
        }

        public Task SendAccountConfirmationTokenPlainAsync(string email, string userName, string token)
        {
            // Simular envio de token de confirmacion de cuenta en texto plano
            Console.WriteLine($"[FAKE EMAIL] Token de confirmación a {email}: {token}");
            return Task.CompletedTask;
        }
    }
}
