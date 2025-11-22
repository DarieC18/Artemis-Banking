using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly ArtemisBankingDbContext _context;

        public LoanRepository(ArtemisBankingDbContext context)
        {
            _context = context;
        }

        public async Task<List<Loan>> GetActiveByUserIdAsync(string userId)
        {
            return await _context.Loans
                .Where(l => l.UserId == userId && l.IsActive)
                .ToListAsync();
        }

        public async Task<Loan?> GetByIdWithScheduleAsync(int loanId)
        {
            return await _context.Loans
                .Include(l => l.TablaAmortizacion)
                .FirstOrDefaultAsync(l => l.Id == loanId);
        }
        public async Task<Loan?> GetByNumberWithScheduleAsync(string loanNumber)
        {
            return await _context.Loans
                .Include(l => l.TablaAmortizacion)
                .FirstOrDefaultAsync(l => l.NumeroPrestamo == loanNumber);
        }
        public async Task UpdateAsync(Loan loan)
        {
            _context.Loans.Update(loan);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Loan>> GetAllActiveAsync()
        {
            return await _context.Loans
                .Where(l => l.IsActive)
                .OrderByDescending(l => l.FechaCreacion)
                .ToListAsync();
        }

        public async Task<List<Loan>> GetAllCompletedAsync()
        {
            return await _context.Loans
                .Where(l => !l.IsActive)
                .OrderByDescending(l => l.FechaCreacion)
                .ToListAsync();
        }

        public async Task<List<Loan>> GetByUserIdAsync(string userId)
        {
            return await _context.Loans
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Loan?> GetByLoanNumberAsync(string loanNumber)
        {
            return await _context.Loans
                .FirstOrDefaultAsync(l => l.NumeroPrestamo == loanNumber);
        }

        public async Task<Loan> AddAsync(Loan loan)
        {
            await _context.Loans.AddAsync(loan);
            await _context.SaveChangesAsync();
            return loan;
        }

        public async Task<decimal> GetAverageDebtAsync()
        {
            var activeLoans = await _context.Loans
                .Where(l => l.IsActive)
                .ToListAsync();

            if (!activeLoans.Any())
                return 0;

            var totalDebt = activeLoans.Sum(l => l.MontoPendiente);
            var uniqueClients = activeLoans.Select(l => l.UserId).Distinct().Count();

            if (uniqueClients == 0)
                return 0;

            return totalDebt / uniqueClients;
        }

        public async Task<bool> HasActiveLoanAsync(string userId)
        {
            return await _context.Loans
                .AnyAsync(l => l.UserId == userId && l.IsActive);
        }

        public async Task<string> GenerateUniqueLoanNumberAsync()
        {
            while (true)
            {
                var number = RandomNumberGenerator.GetInt32(100000000, 999999999).ToString();
                if (!await _context.Loans.AnyAsync(l => l.NumeroPrestamo == number))
                {
                    return number;
                }
            }
        }
    }
}
