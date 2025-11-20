using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Dtos.Loan;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IAdminLoanService
    {
        Task<PaginatedResult<LoanListItemDTO>> GetLoansAsync(
            int pageNumber,
            int pageSize,
            string? statusFilter = null,
            string? cedulaFilter = null,
            CancellationToken cancellationToken = default);

        Task<LoanDetailDTO?> GetLoanByIdAsync(int loanId, CancellationToken cancellationToken = default);

        Task<List<ClientForLoanDTO>> GetEligibleClientsAsync(CancellationToken cancellationToken = default);

        Task<decimal> GetAverageDebtAsync(CancellationToken cancellationToken = default);

        Task<Result> AssignLoanAsync(AssignLoanDTO request, string adminUserId, CancellationToken cancellationToken = default);

        Task<Result> UpdateLoanAsync(UpdateLoanDTO request, CancellationToken cancellationToken = default);
    }
}

