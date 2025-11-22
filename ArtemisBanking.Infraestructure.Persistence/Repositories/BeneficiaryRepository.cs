using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories
{
    public class BeneficiaryRepository : IBeneficiaryRepository
    {
        private readonly ArtemisBankingDbContext _context;

        public BeneficiaryRepository(ArtemisBankingDbContext context)
        {
            _context = context;
        }

        public async Task<List<Beneficiary>> GetByUserIdAsync(string userId)
        {
            return await _context.Beneficiaries
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<Beneficiary?> GetByIdAsync(int id)
        {
            return await _context.Beneficiaries.FindAsync(id);
        }

        public async Task<Beneficiary?> GetByUserAndAccountAsync(string userId, string accountNumber)
        {
            return await _context.Beneficiaries
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId &&
                    b.NumeroCuentaBeneficiario == accountNumber);
        }

        public async Task AddAsync(Beneficiary beneficiary)
        {
            _context.Beneficiaries.Add(beneficiary);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Beneficiary beneficiary)
        {
            _context.Beneficiaries.Remove(beneficiary);
            await _context.SaveChangesAsync();
        }
    }
}
