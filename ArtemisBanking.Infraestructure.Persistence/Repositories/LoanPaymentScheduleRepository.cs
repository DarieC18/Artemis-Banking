using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence.Repositories
{
    public class LoanPaymentScheduleRepository : ILoanPaymentScheduleRepository
    {
        private readonly ArtemisBankingDbContext _context;

        public LoanPaymentScheduleRepository(ArtemisBankingDbContext context)
        {
            _context = context;
        }

        public async Task<List<LoanPaymentSchedule>> GetByLoanIdAsync(int loanId)
        {
            return await _context.LoanPaymentSchedules
                .Where(c => c.LoanId == loanId)
                .OrderBy(c => c.FechaPago)
                .ToListAsync();
        }

        public async Task UpdateRangeAsync(List<LoanPaymentSchedule> cuotas)
        {
            _context.LoanPaymentSchedules.UpdateRange(cuotas);
            await _context.SaveChangesAsync();
        }
    }
}
