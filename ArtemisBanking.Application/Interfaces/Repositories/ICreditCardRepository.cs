using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    public interface ICreditCardRepository
    {
        Task<List<CreditCard>> GetActiveByUserIdAsync(string userId);
        Task<CreditCard?> GetByIdAsync(int id);
        Task<CreditCard?> GetByNumberAsync(string cardNumber);
        Task UpdateAsync(CreditCard card);
        Task<List<CreditCard>> GetAllAsync(string? estadoFilter = null, string? cedulaFilter = null);
        Task<List<CreditCard>> GetByCedulaAsync(string cedula);
        Task<CreditCard> AddAsync(CreditCard card);
        Task<bool> ExistsByNumberAsync(string cardNumber);
        Task<decimal> GetAverageDebtAsync();
    }
}
