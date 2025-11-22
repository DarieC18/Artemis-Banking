using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    public interface IBeneficiaryRepository
    {
        Task<List<Beneficiary>> GetByUserIdAsync(string userId);
        Task<Beneficiary?> GetByIdAsync(int id);
        Task<Beneficiary?> GetByUserAndAccountAsync(string userId, string accountNumber);
        Task AddAsync(Beneficiary beneficiary);
        Task DeleteAsync(Beneficiary beneficiary);
    }
}
