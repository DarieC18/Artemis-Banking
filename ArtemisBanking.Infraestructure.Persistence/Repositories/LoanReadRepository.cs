using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories
{
    public class LoanReadRepository : ILoanReadRepository
    {
        private readonly ArtemisBankingDbContext _context;

        public LoanReadRepository(ArtemisBankingDbContext context)
        {
            _context = context;
        }

        public async Task<Loan?> GetByIdWithScheduleAsync(int loanId, string userId)
        {
            return await _context.Loans
                .Include(l => l.TablaAmortizacion)
                .Where(l => l.Id == loanId
                            && l.UserId == userId
                            && l.IsActive)
                .FirstOrDefaultAsync();
        }
    }
}
