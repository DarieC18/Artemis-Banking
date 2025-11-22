using ArtemisBanking.Application.Dtos.AdminDashboard;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardSummaryDTO> GetSummaryAsync(CancellationToken cancellationToken = default);
    }
}
