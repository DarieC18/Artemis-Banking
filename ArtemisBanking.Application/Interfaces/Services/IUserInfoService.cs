using ArtemisBanking.Application.DTOs.Account;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IUserInfoService
    {
        Task<UserDTO?> GetUserBasicInfoByIdAsync(string userId);
    }
}
