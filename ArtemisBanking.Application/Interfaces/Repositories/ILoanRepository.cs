using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    public interface ILoanRepository
    {
        Task<List<Loan>> GetActiveByUserIdAsync(string userId);
        Task<Loan?> GetByIdWithScheduleAsync(int loanId);
        Task UpdateAsync(Loan loan);
        Task<List<Loan>> GetAllActiveAsync();
        Task<List<Loan>> GetAllCompletedAsync();
        Task<List<Loan>> GetByUserIdAsync(string userId);
        Task<Loan?> GetByLoanNumberAsync(string loanNumber);
        Task<Loan> AddAsync(Loan loan);
        Task<decimal> GetAverageDebtAsync();
        Task<bool> HasActiveLoanAsync(string userId);
        Task<string> GenerateUniqueLoanNumberAsync();
    }
}
