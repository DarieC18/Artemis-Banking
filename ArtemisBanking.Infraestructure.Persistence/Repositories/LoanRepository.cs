using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly ArtemisBankingDbContext _context;

        public LoanRepository(ArtemisBankingDbContext context)
        {
            _context = context;
        }

        public async Task<List<Loan>> GetActiveByUserIdAsync(string userId)
        {
            return await _context.Loans
                .Where(l => l.UserId == userId && l.IsActive)
                .ToListAsync();
        }

        public async Task<Loan?> GetByIdWithScheduleAsync(int loanId)
        {
            return await _context.Loans
                .Include(l => l.TablaAmortizacion)
                .FirstOrDefaultAsync(l => l.Id == loanId);
        }

        public async Task UpdateAsync(Loan loan)
        {
            _context.Loans.Update(loan);
            await _context.SaveChangesAsync();
        }
    }
}
