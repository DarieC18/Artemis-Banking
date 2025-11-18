using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Dtos.CreditCard;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IAdminCreditCardService
    {
        Task<PaginatedResult<CreditCardListItemDTO>> GetCreditCardsAsync(
            int pageNumber,
            int pageSize,
            string? estadoFilter = null,
            string? cedulaFilter = null,
            CancellationToken cancellationToken = default);

        Task<CreditCardDetailDTO?> GetCreditCardByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<List<ClientForCreditCardDTO>> GetEligibleClientsAsync(CancellationToken cancellationToken = default);

        Task<decimal> GetAverageDebtAsync(CancellationToken cancellationToken = default);

        Task<Result> AssignCreditCardAsync(AssignCreditCardDTO request, string adminUserId, CancellationToken cancellationToken = default);

        Task<Result> UpdateCreditCardLimitAsync(UpdateCreditCardLimitDTO request, CancellationToken cancellationToken = default);

        Task<Result> CancelCreditCardAsync(int id, CancellationToken cancellationToken = default);
    }
}
