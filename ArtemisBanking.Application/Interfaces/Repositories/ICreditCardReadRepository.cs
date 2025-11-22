using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    public interface ICreditCardReadRepository
    {
        Task<CreditCard?> GetByIdWithConsumptionsAsync(int creditCardId, string userId);
    }
}
