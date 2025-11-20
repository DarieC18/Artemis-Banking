using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    public interface ISavingsAccountReadRepository
    {
        Task<SavingsAccount?> GetByIdWithTransactionsAsync(int cuentaId, string userId);
    }
}
