using ArtemisBanking.Application.Dtos.Beneficiary;
using ArtemisBanking.Application.ViewModels.Cliente;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IBeneficiaryService
    {
        Task<List<BeneficiaryDTO>> GetBeneficiariesAsync(string userId);
        Task AddBeneficiaryAsync(string userId, AddBeneficiaryViewModel model);
        Task DeleteBeneficiaryAsync(string userId, int beneficiaryId);
    }
}
