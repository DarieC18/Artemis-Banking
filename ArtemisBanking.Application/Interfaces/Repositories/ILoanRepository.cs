using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    public interface ILoanRepository
    {
        Task<List<Loan>> GetActiveByUserIdAsync(string userId);
        Task<Loan?> GetByIdWithScheduleAsync(int loanId);
        Task UpdateAsync(Loan loan);
    }
}
