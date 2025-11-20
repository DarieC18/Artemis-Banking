using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ArtemisBankingDbContext _context;

        public TransactionRepository(ArtemisBankingDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Transaction>> GetByAccountNumberAsync(string accountNumber)
        {
            return await _context.Transactions
                .Include(t => t.SavingsAccount)
                .Where(t => t.SavingsAccount.NumeroCuenta == accountNumber)
                .OrderByDescending(t => t.FechaTransaccion)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Transactions.CountAsync();
        }

        public async Task<int> GetTodayCountAsync(DateTime today, DateTime todayEnd)
        {
            return await _context.Transactions
                .Where(t => t.FechaTransaccion >= today && t.FechaTransaccion < todayEnd)
                .CountAsync();
        }

        public async Task<int> GetTodayPaymentsCountAsync(DateTime today, DateTime todayEnd)
        {
            return await _context.Transactions
                .Where(t => t.FechaTransaccion >= today && t.FechaTransaccion < todayEnd &&
                           (t.Tipo == "Pago de préstamo" || t.Tipo == "Pago de tarjeta"))
                .CountAsync();
        }

        public async Task<int> GetTotalPaymentsCountAsync()
        {
            return await _context.Transactions
                .Where(t => t.Tipo == "Pago de préstamo" || t.Tipo == "Pago de tarjeta")
                .CountAsync();
        }
    }
}
