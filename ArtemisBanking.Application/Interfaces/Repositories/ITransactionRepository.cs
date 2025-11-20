using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction);
        Task<List<Transaction>> GetByAccountNumberAsync(string accountNumber);
        Task<int> GetTotalCountAsync();
        Task<int> GetTodayCountAsync(DateTime today, DateTime todayEnd);
        Task<int> GetTodayPaymentsCountAsync(DateTime today, DateTime todayEnd);
        Task<int> GetTotalPaymentsCountAsync();
    }
}
