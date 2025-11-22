using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Dtos.SavingsAccount;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IAdminSavingsAccountService
    {
        Task<PaginatedResult<SavingsAccountListItemDTO>> GetSavingsAccountsAsync(
            int pageNumber,
            int pageSize,
            string? estadoFilter = null,
            string? tipoFilter = null,
            string? cedulaFilter = null,
            CancellationToken cancellationToken = default);

        Task<List<ClientForSavingsAccountDTO>> GetEligibleClientsAsync(CancellationToken cancellationToken = default);

        Task<Result> AssignSecondaryAccountAsync(AssignSavingsAccountDTO request, string adminUserId, CancellationToken cancellationToken = default);

        Task<SavingsAccountDetailDTO?> GetAccountDetailAsync(int accountId, CancellationToken cancellationToken = default);

        Task<Result> CancelSecondaryAccountAsync(int accountId, string adminUserId, CancellationToken cancellationToken = default);
    }
}

