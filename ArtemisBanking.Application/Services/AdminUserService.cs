using System.Security.Cryptography;
using AutoMapper;
using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Dtos.AdminUsers;
using ArtemisBanking.Application.Dtos.Identity;
using ArtemisBanking.Application.Dtos.SavingsAccount;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Interfaces.Identity;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Services
{
    public class AdminUserService : IAdminUserService
    {
        private static readonly string[] SupportedRoles = { "Administrador", "Cajero", "Cliente" };
        private readonly IIdentityUserManager _identityUserManager;
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public AdminUserService(
            IIdentityUserManager identityUserManager,
            ISavingsAccountRepository savingsAccountRepository,
            ILoanRepository loanRepository,
            IEmailService emailService,
            IMapper mapper)
        {
            _identityUserManager = identityUserManager;
            _savingsAccountRepository = savingsAccountRepository;
            _loanRepository = loanRepository;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<AdminUserListItemDTO>> GetUsersAsync(int pageNumber, int pageSize, string? roleFilter = null, CancellationToken cancellationToken = default)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 20;

            var allUsers = await _identityUserManager.GetAllAsync(cancellationToken);

            var filteredUsers = allUsers
                .Where(u => !u.Roles.Any(r => r.Equals("Comercio", StringComparison.OrdinalIgnoreCase)))
                .Where(u => string.IsNullOrWhiteSpace(roleFilter) || !SupportedRoles.Contains(roleFilter) || u.Roles.Contains(roleFilter))
                .OrderByDescending(u => u.FechaCreacion)
                .ToList();

            var totalCount = filteredUsers.Count;
            var pagedUsers = filteredUsers
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtoList = _mapper.Map<List<AdminUserListItemDTO>>(pagedUsers);

            return new PaginatedResult<AdminUserListItemDTO>(dtoList, pageNumber, pageSize, totalCount);
        }

        public async Task<AdminUserListItemDTO?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _identityUserManager.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            return _mapper.Map<AdminUserListItemDTO>(user);
        }

        public async Task<Result> CreateUserAsync(CreateAdminUserDTO request, CancellationToken cancellationToken = default)
        {
            if (!SupportedRoles.Contains(request.Role))
            {
                return Result.Fail("Rol inválido.");
            }

            if (await _identityUserManager.FindByUserNameAsync(request.UserName, cancellationToken) != null)
            {
                return Result.Fail("El nombre de usuario ya está en uso.");
            }

            if (await _identityUserManager.FindByEmailAsync(request.Email, cancellationToken) != null)
            {
                return Result.Fail("El correo electrónico ya está en uso.");
            }

            var createCommand = new CreateIdentityUserCommand
            {
                UserName = request.UserName,
                Email = request.Email,
                Cedula = request.Cedula,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                IsActive = false,
                EmailConfirmed = false,
                FechaCreacionUtc = DateTime.UtcNow
            };

            var createResult = await _identityUserManager.CreateAsync(createCommand, request.Password, cancellationToken);
            if (createResult.IsFailure)
            {
                return createResult.Errors != null && createResult.Errors.Any()
                    ? Result.Fail(createResult.Errors)
                    : Result.Fail(createResult.GeneralError ?? "No se pudo crear el usuario.");
            }

            var userId = createResult.Value!;

            if (!await _identityUserManager.RoleExistsAsync(request.Role, cancellationToken))
            {
                return Result.Fail($"El rol {request.Role} no existe");
            }

            var addRoleResult = await _identityUserManager.AddToRoleAsync(userId, request.Role, cancellationToken);
            if (addRoleResult.IsFailure)
            {
                return addRoleResult.Errors != null && addRoleResult.Errors.Any()
                    ? Result.Fail(addRoleResult.Errors)
                    : Result.Fail(addRoleResult.GeneralError ?? "No se pudo asignar el rol");
            }

            if (request.Role == "Cliente")
            {
                var montoInicial = request.MontoInicial ?? 0m;
                var numeroCuenta = await GenerateUniqueAccountNumberAsync(cancellationToken);

                var newAccountDto = new CreateSavingsAccountDTO
                {
                    NumeroCuenta = numeroCuenta,
                    Balance = montoInicial,
                    EsPrincipal = true,
                    IsActive = true,
                    FechaCreacion = DateTime.UtcNow,
                    UserId = userId
                };

                var entity = _mapper.Map<SavingsAccount>(newAccountDto);
                await _savingsAccountRepository.AddAsync(entity, cancellationToken);
            }

            var tokenResult = await _identityUserManager.GenerateEmailConfirmationTokenAsync(userId, cancellationToken);
            if (tokenResult.IsFailure)
            {
                return tokenResult.Errors != null && tokenResult.Errors.Any()
                    ? Result.Fail(tokenResult.Errors)
                    : Result.Fail(tokenResult.GeneralError ?? "No se pudo generar el token de confirmación.");
            }

            await _emailService.SendAccountConfirmationEmailAsync(request.Email, request.UserName, tokenResult.Value!);

            return Result.Ok();
        }

        public async Task<Result> UpdateUserAsync(UpdateAdminUserDTO request, CancellationToken cancellationToken = default)
        {
            var user = await _identityUserManager.GetByIdAsync(request.Id, cancellationToken);
            if (user == null)
            {
                return Result.Fail("Usuario no encontrado.");
            }

            var updateCommand = new UpdateIdentityUserCommand
            {
                Id = request.Id,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Cedula = request.Cedula,
                Email = request.Email,
                UserName = request.UserName
            };

            var updateResult = await _identityUserManager.UpdateAsync(updateCommand, cancellationToken);
            if (updateResult.IsFailure)
            {
                return updateResult.Errors != null && updateResult.Errors.Any()
                    ? Result.Fail(updateResult.Errors)
                    : Result.Fail(updateResult.GeneralError ?? "No se pudo actualizar el usuario");
            }

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var passwordResult = await _identityUserManager.ResetPasswordAsync(request.Id, request.Password, cancellationToken);
                if (passwordResult.IsFailure)
                {
                    return passwordResult.Errors != null && passwordResult.Errors.Any()
                        ? Result.Fail(passwordResult.Errors)
                        : Result.Fail(passwordResult.GeneralError ?? "No se pudo actualizar la contraseña");
                }
            }

            var roles = await _identityUserManager.GetRolesAsync(request.Id, cancellationToken);
            if (roles.Contains("Cliente") && request.MontoAdicional.HasValue && request.MontoAdicional.Value > 0)
            {
                var cuentaPrincipal = await _savingsAccountRepository.GetPrincipalByUserIdAsync(user.Id);
                if (cuentaPrincipal != null)
                {
                    cuentaPrincipal.Balance += request.MontoAdicional.Value;
                    await _savingsAccountRepository.UpdateAsync(cuentaPrincipal);
                }
            }

            return Result.Ok();
        }

        public async Task<Result> ToggleUserStatusAsync(string userId, bool activate, string currentAdminId, CancellationToken cancellationToken = default)
        {
            if (userId == currentAdminId)
            {
                return Result.Fail("No puedes cambiar tu propio estado");
            }

            var user = await _identityUserManager.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result.Fail("Usuario no encontrado.");
            }

            var result = await _identityUserManager.SetActiveStateAsync(userId, activate, cancellationToken);
            if (result.IsFailure)
            {
                return result.Errors != null && result.Errors.Any()
                    ? Result.Fail(result.Errors)
                    : Result.Fail(result.GeneralError ?? "No se pudo actualizar el estado del usuario");
            }

            if (!activate)
            {
                var tokenResult = await _identityUserManager.GeneratePasswordResetTokenAsync(userId, cancellationToken);
                if (tokenResult.IsFailure)
                {
                    return tokenResult.Errors != null && tokenResult.Errors.Any()
                        ? Result.Fail(tokenResult.Errors)
                        : Result.Fail(tokenResult.GeneralError ?? "No se pudo generar el token de restablecimiento");
                }

                var expiry = DateTime.UtcNow.AddHours(24);
                var setTokenResult = await _identityUserManager.SetPasswordResetTokenAsync(userId, tokenResult.Value!, expiry, cancellationToken);
                if (setTokenResult.IsFailure)
                {
                    return setTokenResult.Errors != null && setTokenResult.Errors.Any()
                        ? Result.Fail(setTokenResult.Errors)
                        : Result.Fail(setTokenResult.GeneralError ?? "No se pudo asignar el token de restablecimiento");
                }
            }

            return Result.Ok();
        }

        private async Task<string> GenerateUniqueAccountNumberAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                var number = RandomNumberGenerator.GetInt32(100000000, 999999999).ToString();
                var existsInAccounts = await _savingsAccountRepository.ExistsByAccountNumberAsync(number);
                var existsInLoans = await _loanRepository.GetByLoanNumberAsync(number) != null;

                if (!existsInAccounts && !existsInLoans)
                {
                    return number;
                }
            }
        }
    }
}
