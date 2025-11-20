using ArtemisBanking.Application.Dtos.Transaction;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IAccountCashOperationsService
    {
        Task<Result<DepositPreviewDTO>> PreviewDepositAsync(DepositDTO request);
        Task<Result<DepositResultDTO>> ExecuteDepositAsync(DepositDTO command);
        Task<Result<WithdrawalPreviewDTO>> PreviewWithdrawalAsync(WithdrawalDTO request);
        Task<Result<WithdrawalResultDTO>> ExecuteWithdrawalAsync(WithdrawalDTO command);

    }
}
