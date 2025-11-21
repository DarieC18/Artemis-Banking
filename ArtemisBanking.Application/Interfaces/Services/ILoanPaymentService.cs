using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Dtos.Transaction;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface ILoanPaymentService
    {
        Task<Result<PayLoanPreviewDTO>> PreviewPayLoanAsync(PayLoanDTO request);
        Task<Result<PayLoanResultDTO>> ExecutePayLoanAsync(PayLoanDTO command);
    }
}
