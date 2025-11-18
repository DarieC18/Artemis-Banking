using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories
{
    public class CreditCardConsumptionRepository : ICreditCardConsumptionRepository
    {
        private readonly ArtemisBankingDbContext _context;

        public CreditCardConsumptionRepository(ArtemisBankingDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CreditCardConsumption consumption)
        {
            await _context.CreditCardConsumptions.AddAsync(consumption);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CreditCardConsumption>> GetByCardIdAsync(int creditCardId)
        {
            return await _context.CreditCardConsumptions
                .Where(c => c.CreditCardId == creditCardId)
                .OrderByDescending(c => c.FechaConsumo)
                .ToListAsync();
        }
    }
}
