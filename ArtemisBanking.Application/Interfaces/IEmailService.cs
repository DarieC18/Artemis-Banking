using ArtemisBanking.Application.DTOs.Email;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);
        Task SendAccountConfirmationEmailAsync(string email, string userName, string token);
        Task SendPasswordResetEmailAsync(string email, string userName, string token);
        Task SendAccountConfirmationTokenPlainAsync(string email, string userName, string token);

    }
}
