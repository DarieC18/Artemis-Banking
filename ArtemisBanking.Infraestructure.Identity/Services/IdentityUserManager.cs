using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Dtos.Identity;
using ArtemisBanking.Application.Interfaces.Identity;
using ArtemisBanking.Infraestructure.Identity.Entities;
using ArtemisBanking.Infraestructure.Identity.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infraestructure.Identity.Services
{
    public class IdentityUserManager : IIdentityUserManager
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IdentityContext _context;
        private readonly IMapper _mapper;

        public IdentityUserManager(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IdentityContext context,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<IdentityUserDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userManager.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var result = new List<IdentityUserDto>(users.Count);
            foreach (var user in users)
            {
                var dto = await MapToDtoAsync(user);
                result.Add(dto);
            }

            return result;
        }

        public async Task<IdentityUserDto?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user == null ? null : await MapToDtoAsync(user);
        }

        public async Task<IdentityUserDto?> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByNameAsync(userName);
            return user == null ? null : await MapToDtoAsync(user);
        }

        public async Task<IdentityUserDto?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user == null ? null : await MapToDtoAsync(user);
        }

        public async Task<Result<string>> CreateAsync(CreateIdentityUserCommand command, string password, CancellationToken cancellationToken = default)
        {
            var user = _mapper.Map<AppUser>(command);

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                return BuildFailure<string>(createResult);
            }

            return Result<string>.Ok(user.Id);
        }

        public async Task<Result> UpdateAsync(UpdateIdentityUserCommand command, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(command.Id);
            if (user == null)
            {
                return Result.Fail("Usuario no encontrado.");
            }

            _mapper.Map(command, user);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BuildFailure(updateResult);
            }

            return Result.Ok();
        }

        public Task<bool> RoleExistsAsync(string roleName, CancellationToken cancellationToken = default)
        {
            return _roleManager.RoleExistsAsync(roleName);
        }

        public async Task<Result> AddToRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail("Usuario no encontrado.");
            }

            var addResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!addResult.Succeeded)
            {
                return BuildFailure(addResult);
            }

            return Result.Ok();
        }

        public async Task<IList<string>> GetRolesAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<string>();
            }

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<Result> ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail("Usuario no encontrado.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!resetResult.Succeeded)
            {
                return BuildFailure(resetResult);
            }

            return Result.Ok();
        }

        public async Task<Result> SetActiveStateAsync(string userId, bool isActive, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
            {
                return Result.Fail("Usuario no encontrado.");
            }

            user.IsActive = isActive;
            _context.Entry(user).Property(u => u.IsActive).IsModified = true;
            var saved = await _context.SaveChangesAsync(cancellationToken);
            
            if (saved > 0)
            {
                return Result.Ok();
            }
            
            return Result.Fail("No se pudo guardar el cambio de estado.");
        }

        public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<string>.Fail("Usuario no encontrado.");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return Result<string>.Ok(token);
        }

        public async Task<Result<string>> GeneratePasswordResetTokenAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<string>.Fail("Usuario no encontrado.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return Result<string>.Ok(token);
        }

        public async Task<Result> SetPasswordResetTokenAsync(string userId, string token, DateTime expiryUtc, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail("Usuario no encontrado.");
            }

            user.ResetPasswordToken = token;
            user.ResetPasswordTokenExpiry = expiryUtc;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BuildFailure(updateResult);
            }

            return Result.Ok();
        }

        private async Task<IdentityUserDto> MapToDtoAsync(AppUser user)
        {
            var dto = _mapper.Map<IdentityUserDto>(user);
            dto.Roles = await _userManager.GetRolesAsync(user);
            return dto;
        }

        private static Result BuildFailure(IdentityResult identityResult)
        {
            var errors = identityResult.Errors.Select(e => e.Description).ToList();
            return errors.Count > 0 ? Result.Fail(errors) : Result.Fail("Operación fallida.");
        }

        private static Result<T> BuildFailure<T>(IdentityResult identityResult)
        {
            var errors = identityResult.Errors.Select(e => e.Description).ToList();
            return errors.Count > 0 ? Result<T>.Fail(errors) : Result<T>.Fail("Operación fallida.");
        }
    }
}
