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
                .OrderByDescending(a => a.FechaCreacion)
                .ToListAsync();
        }

        public async Task<List<SavingsAccount>> GetByUserIdIncludingInactiveAsync(string userId)
        {
            return await _context.SavingsAccounts
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.FechaCreacion)
                .ToListAsync();
        }

        public async Task<List<SavingsAccount>> GetAllAsync()
        {
            return await _context.SavingsAccounts
                .OrderByDescending(a => a.FechaCreacion)
                .ToListAsync();
        }

        public async Task<SavingsAccount?> GetByIdAsync(int id)
        {
            return await _context.SavingsAccounts
                .FirstOrDefaultAsync(a => a.Id == id);
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

        public async Task<bool> ExistsByAccountNumberAsync(string accountNumber)
        {
            return await _context.SavingsAccounts
                .AnyAsync(a => a.NumeroCuenta == accountNumber);
        }

        public async Task UpdateAsync(SavingsAccount account)
        {
            _context.SavingsAccounts.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(SavingsAccount account, CancellationToken cancellationToken = default)
        {
            await _context.SavingsAccounts.AddAsync(account, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
