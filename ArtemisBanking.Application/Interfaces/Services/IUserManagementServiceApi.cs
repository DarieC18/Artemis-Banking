using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs.Common;
using ArtemisBanking.Application.DTOs.Users;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IUserManagementServiceApi
    {
        Task<Result<PagedResult<UserListApiDto>>> GetUsersAsync(int? page, int? pageSize, string? rol);
        Task<Result<PagedResult<UserListApiDto>>> GetCommerceUsersAsync(int? page, int? pageSize);
        Task<Result<UserDetailApiDto>> GetByIdAsync(string id);

        Task<Result<string>> CreateAsync(CreateUserApiDto dto);
        Task<Result<string>> CreateCommerceUserAsync(int commerceId, CreateCommerceUserApiDto dto);

        Task<Result> UpdateAsync(string id, UpdateUserApiDto dto);
        Task<Result> ChangeStatusAsync(string id, bool status);
    }
}
