using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Dtos.AdminUsers;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IAdminUserService
    {
        Task<PaginatedResult<AdminUserListItemDTO>> GetUsersAsync(
            int pageNumber,
            int pageSize,
            string? roleFilter = null,
            CancellationToken cancellationToken = default);

        Task<Result> CreateUserAsync(CreateAdminUserDTO request, CancellationToken cancellationToken = default);
        Task<Result> UpdateUserAsync(UpdateAdminUserDTO request, CancellationToken cancellationToken = default);
        Task<Result> ToggleUserStatusAsync(string userId, bool activate, string currentAdminId, CancellationToken cancellationToken = default);
        Task<AdminUserListItemDTO?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    }
}
