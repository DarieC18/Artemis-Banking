using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    public interface ICreditCardConsumptionRepository
    {
        Task AddAsync(CreditCardConsumption consumption);
        Task<List<CreditCardConsumption>> GetByCardIdAsync(int creditCardId);
    }
}
