using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    public interface ILoanReadRepository
    {
        Task<Loan?> GetByIdWithScheduleAsync(int loanId, string userId);
    }
}
