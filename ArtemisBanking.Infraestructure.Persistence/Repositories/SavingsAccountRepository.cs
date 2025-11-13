using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories
{
    public class SavingsAccountRepository : ISavingsAccountRepository
    {
        private readonly ArtemisBankingDbContext _context;

        public SavingsAccountRepository(ArtemisBankingDbContext context)
        {
            _context = context;
        }

        public async Task<List<SavingsAccount>> GetByUserIdAsync(string userId)
        {
            return await _context.SavingsAccounts
                .Where(a => a.UserId == userId && a.IsActive)
                .ToListAsync();
        }

        public async Task<SavingsAccount?> GetPrincipalByUserIdAsync(string userId)
        {
            return await _context.SavingsAccounts
                .FirstOrDefaultAsync(a => a.UserId == userId && a.EsPrincipal && a.IsActive);
        }

        public async Task<SavingsAccount?> GetByAccountNumberAsync(string accountNumber)
        {
            return await _context.SavingsAccounts
                .FirstOrDefaultAsync(a => a.NumeroCuenta == accountNumber && a.IsActive);
        }

        public async Task UpdateAsync(SavingsAccount account)
        {
            _context.SavingsAccounts.Update(account);
            await _context.SaveChangesAsync();
        }
    }
}
