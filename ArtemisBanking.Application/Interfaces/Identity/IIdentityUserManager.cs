using ArtemisBanking.Application.Dtos.Identity;
using ArtemisBanking.Application.Common;

namespace ArtemisBanking.Application.Interfaces.Identity
{
    public interface IIdentityUserManager
    {
        Task<IReadOnlyList<IdentityUserDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IdentityUserDto?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IdentityUserDto?> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<IdentityUserDto?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<Result<string>> CreateAsync(CreateIdentityUserCommand command, string password, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(UpdateIdentityUserCommand command, CancellationToken cancellationToken = default);

        Task<bool> RoleExistsAsync(string roleName, CancellationToken cancellationToken = default);
        Task<Result> AddToRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);
        Task<IList<string>> GetRolesAsync(string userId, CancellationToken cancellationToken = default);

        Task<Result> ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default);
        Task<Result> SetActiveStateAsync(string userId, bool isActive, CancellationToken cancellationToken = default);

        Task<Result<string>> GenerateEmailConfirmationTokenAsync(string userId, CancellationToken cancellationToken = default);
        Task<Result<string>> GeneratePasswordResetTokenAsync(string userId, CancellationToken cancellationToken = default);
        Task<Result> SetPasswordResetTokenAsync(string userId, string token, DateTime expiryUtc, CancellationToken cancellationToken = default);
    }
}
