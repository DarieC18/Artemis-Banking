using ArtemisBanking.Application.DTOs.Account;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IAccountService
    {
        Task<AuthenticationResultDTO> AuthenticateAsync(LoginDTO loginDto);
        Task SignOutAsync();
        Task<ServiceResultDTO> RequestPasswordResetAsync(ForgotPasswordDTO forgotPasswordDto);
        Task<ServiceResultDTO> ResetPasswordAsync(ResetPasswordDTO resetPasswordDto);
        Task<ServiceResultDTO> ResetPasswordByIdAsync(ResetPasswordByIdDTO resetPasswordDto);
        Task<ServiceResultDTO> ConfirmAccountAsync(string token);
        Task<IList<string>> GetUserRolesAsync(string userId);
    }
}
