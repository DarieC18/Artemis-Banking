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
            return await _context.CreditCards
                .Include(c => c.Consumos)
                .FirstOrDefaultAsync(c => c.Id == id);
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

        public async Task<List<CreditCard>> GetAllAsync(string? estadoFilter = null, string? cedulaFilter = null)
        {
            var query = _context.CreditCards
                .Include(c => c.Consumos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estadoFilter))
            {
                if (estadoFilter == "ACTIVA")
                {
                    query = query.Where(c => c.IsActive);
                }
                else if (estadoFilter == "CANCELADA")
                {
                    query = query.Where(c => !c.IsActive);
                }
            }

            if (!string.IsNullOrWhiteSpace(cedulaFilter))
            {
                // Necesitamos hacer join con Identity para buscar por cedula
                // Por ahora, retornamos todas y filtramos después
                query = query.AsQueryable();
            }

            return await query.OrderByDescending(c => c.FechaCreacion).ToListAsync();
        }

        public async Task<List<CreditCard>> GetByCedulaAsync(string cedula)
        {
            // Esto requiere un join con Identity, por ahora retornamos todas
            // El servicio se encargara del filtrado por cedula
            return await _context.CreditCards
                .Include(c => c.Consumos)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();
        }

        public async Task<CreditCard> AddAsync(CreditCard card)
        {
            await _context.CreditCards.AddAsync(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task<bool> ExistsByNumberAsync(string cardNumber)
        {
            return await _context.CreditCards
                .AnyAsync(c => c.NumeroTarjeta == cardNumber);
        }

        public async Task<decimal> GetAverageDebtAsync()
        {
            var activeCards = await _context.CreditCards
                .Where(c => c.IsActive)
                .ToListAsync();

            if (!activeCards.Any())
                return 0;

            var totalDebt = activeCards.Sum(c => c.DeudaActual);
            var uniqueUsers = activeCards.Select(c => c.UserId).Distinct().Count();

            return uniqueUsers > 0 ? totalDebt / uniqueUsers : 0;
        }
    }
}
