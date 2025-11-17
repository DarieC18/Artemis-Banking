using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories
{
    public class SavingsAccountReadRepository : ISavingsAccountReadRepository
    {
        private readonly ArtemisBankingDbContext _context;

        public SavingsAccountReadRepository(ArtemisBankingDbContext context)
        {
            _context = context;
        }

        public async Task<SavingsAccount?> GetByIdWithTransactionsAsync(int cuentaId, string userId)
        {
            return await _context.SavingsAccounts
                .Include(c => c.Transactions)
                .Where(c => c.Id == cuentaId
                            && c.UserId == userId
                            && c.IsActive)
                .FirstOrDefaultAsync();
        }
    }
}
