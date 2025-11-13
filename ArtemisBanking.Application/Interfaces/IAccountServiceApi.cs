using ArtemisBanking.Application.DTOs.Account;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IAccountServiceApi
    {
        Task<ServiceResultWithDataDTO<JwtResponseDTO>> LoginAsync(LoginDTO loginDto);
        Task<ServiceResultDTO> ConfirmAccountAsync(ConfirmAccountApiRequestDTO request);
        Task<ServiceResultDTO> GenerateResetTokenAsync(ForgotPasswordApiRequestDTO request);
        Task<ServiceResultDTO> ResetPasswordByIdAsync(ResetPasswordByIdDTO request);
    }
}
