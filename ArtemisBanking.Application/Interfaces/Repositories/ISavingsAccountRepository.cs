using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    public interface ISavingsAccountRepository
    {
        Task<List<SavingsAccount>> GetByUserIdAsync(string userId);
        Task<List<SavingsAccount>> GetByUserIdIncludingInactiveAsync(string userId);
        Task<List<SavingsAccount>> GetAllAsync();
        Task<SavingsAccount?> GetByIdAsync(int id);
        Task<SavingsAccount?> GetPrincipalByUserIdAsync(string userId);
        Task<SavingsAccount?> GetByAccountNumberAsync(string accountNumber);
        Task<bool> ExistsByAccountNumberAsync(string accountNumber);
        Task UpdateAsync(SavingsAccount account);
        Task AddAsync(SavingsAccount account, CancellationToken cancellationToken = default);
    }
}
