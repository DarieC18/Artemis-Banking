using ArtemisBanking.Application.DTOs.Common;
using ArtemisBanking.Application.DTOs.Hermes;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IHermesPayService
    {
        Task<Result<PagedResult<CommerceTransactionDto>>> GetTransactionsAsync(
            int commerceId,
            int page,
            int pageSize);

        Task<Result> ProcessPaymentAsync(
            int commerceId,
            ProcessPaymentRequestDto request);
    }
}
