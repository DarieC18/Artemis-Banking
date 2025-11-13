using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    public interface ILoanPaymentScheduleRepository
    {
        Task<List<LoanPaymentSchedule>> GetByLoanIdAsync(int loanId);
        Task UpdateRangeAsync(List<LoanPaymentSchedule> cuotas);
    }
}
