using ArtemisBanking.Application.DTOs.Account;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infraestructure.Identity.Services
{
    public class UserInfoService : IUserInfoService
    {
        private readonly UserManager<AppUser> _userManager;

        public UserInfoService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserDTO?> GetUserBasicInfoByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || !user.IsActive)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Cedula = user.Cedula,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                IsActive = user.IsActive,
                FechaCreacion = user.FechaCreacion,
                Roles = roles
            };
        }

        public async Task<UserDTO?> GetUserBasicInfoByCommerceIdAsync(int commerceId)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.CommerceId == commerceId && u.IsActive);

            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Cedula = user.Cedula,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                IsActive = user.IsActive,
                FechaCreacion = user.FechaCreacion,
                Roles = roles
            };
        }
    }
}
