using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    public interface ICreditCardRepository
    {
        Task<List<CreditCard>> GetActiveByUserIdAsync(string userId);
        Task<CreditCard?> GetByIdAsync(int id);
        Task<CreditCard?> GetByNumberAsync(string cardNumber);
        Task UpdateAsync(CreditCard card);
    }
}
