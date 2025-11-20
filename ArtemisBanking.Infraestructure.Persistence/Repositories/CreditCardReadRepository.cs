using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories
{
    public class CreditCardReadRepository : ICreditCardReadRepository
    {
        private readonly ArtemisBankingDbContext _context;

        public CreditCardReadRepository(ArtemisBankingDbContext context)
        {
            _context = context;
        }

        public async Task<CreditCard?> GetByIdWithConsumptionsAsync(int creditCardId, string userId)
        {
            return await _context.CreditCards
                .Include(c => c.Consumos)
                .Where(c => c.Id == creditCardId
                            && c.UserId == userId
                            && c.IsActive)
                .FirstOrDefaultAsync();
        }
    }
}
