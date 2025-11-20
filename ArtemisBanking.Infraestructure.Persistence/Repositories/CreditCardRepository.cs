using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories
{
    public class CreditCardRepository : ICreditCardRepository
    {
        private readonly ArtemisBankingDbContext _context;

        public CreditCardRepository(ArtemisBankingDbContext context)
        {
            _context = context;
        }

        public async Task<List<CreditCard>> GetActiveByUserIdAsync(string userId)
        {
            return await _context.CreditCards
                .Where(c => c.UserId == userId && c.IsActive)
                .ToListAsync();
        }

        public async Task<CreditCard?> GetByIdAsync(int id)
        {
            return await _context.CreditCards.FindAsync(id);
        }

        public async Task<CreditCard?> GetByNumberAsync(string cardNumber)
        {
            return await _context.CreditCards
                .FirstOrDefaultAsync(c => c.NumeroTarjeta == cardNumber && c.IsActive);
        }

        public async Task UpdateAsync(CreditCard card)
        {
            _context.CreditCards.Update(card);
            await _context.SaveChangesAsync();
        }
    }
}
