using ArtemisBanking.Application;
using ArtemisBanking.Application.DTOs.Common;
using ArtemisBanking.Application.DTOs.Users;
using ArtemisBanking.Application.Interfaces.Persistence;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infraestructure.Identity.Services
{
    public class UserManagementServiceApi : IUserManagementServiceApi
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ISavingsAccountRepository _savingsRepo;
        private readonly IGenericRepository<SavingsAccount> _accounts;
        private readonly IGenericRepository<Commerce> _commerceRepo;


        public UserManagementServiceApi(UserManager<AppUser> userManager, ISavingsAccountRepository savingsRepo, IGenericRepository<SavingsAccount> accounts, IGenericRepository<Commerce> commerceRepo)
        {
            _userManager = userManager;
            _savingsRepo = savingsRepo;
            _accounts = accounts;
            _commerceRepo = commerceRepo;
        }

        private async Task<UserListApiDto> MapToListDtoAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            return new UserListApiDto
            {
                Usuario = user.UserName ?? string.Empty,
                Cedula = user.Cedula,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Correo = user.Email ?? string.Empty,
                Rol = role.ToLowerInvariant(),
                Estado = user.IsActive ? "activo" : "inactivo"
            };
        }

        public async Task<Result<PagedResult<UserListApiDto>>> GetUsersAsync(int? page, int? pageSize, string? rol)
        {
            var usuariosQuery = _userManager.Users
                .OrderByDescending(u => u.FechaCreacion);

            var usuarios = await usuariosQuery.ToListAsync();

            var filtrados = new List<AppUser>();

            foreach (var user in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Comercio"))
                    continue;

                if (!string.IsNullOrWhiteSpace(rol))
                {
                    var matchRol = roles.Any(r =>
                        string.Equals(r, rol, StringComparison.OrdinalIgnoreCase));

                    if (!matchRol)
                        continue;
                }

                filtrados.Add(user);
            }

            var dtoList = new List<UserListApiDto>();
            foreach (var user in filtrados)
            {
                dtoList.Add(await MapToListDtoAsync(user));
            }

            var totalCount = dtoList.Count;

            if (page is null || pageSize is null || page <= 0 || pageSize <= 0)
            {
                var all = new PagedResult<UserListApiDto>
                {
                    Data = dtoList,
                    CurrentPage = 1,
                    TotalPages = 1,
                    TotalCount = totalCount
                };

                return Result<PagedResult<UserListApiDto>>.Ok(all);
            }

            var currentPage = page.Value;
            var size = pageSize.Value;

            var pageData = dtoList
                .Skip((currentPage - 1) * size)
                .Take(size)
                .ToList();

            var paged = new PagedResult<UserListApiDto>
            {
                Data = pageData,
                CurrentPage = currentPage,
                TotalPages = (int)Math.Ceiling(totalCount / (double)size),
                TotalCount = totalCount
            };

            return Result<PagedResult<UserListApiDto>>.Ok(paged);
        }

        public async Task<Result<PagedResult<UserListApiDto>>> GetCommerceUsersAsync(int? page, int? pageSize)
        {
            var usuariosQuery = _userManager.Users
                .OrderByDescending(u => u.FechaCreacion);

            var usuarios = await usuariosQuery.ToListAsync();

            var filtrados = new List<AppUser>();

            foreach (var user in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (!roles.Contains("Comercio"))
                    continue;

                filtrados.Add(user);
            }

            var dtoList = new List<UserListApiDto>();
            foreach (var user in filtrados)
            {
                dtoList.Add(await MapToListDtoAsync(user));
            }

            var totalCount = dtoList.Count;

            if (page is null || pageSize is null || page <= 0 || pageSize <= 0)
            {
                var all = new PagedResult<UserListApiDto>
                {
                    Data = dtoList,
                    CurrentPage = 1,
                    TotalPages = 1,
                    TotalCount = totalCount
                };

                return Result<PagedResult<UserListApiDto>>.Ok(all);
            }

            var currentPage = page.Value;
            var size = pageSize.Value;

            var pageData = dtoList
                .Skip((currentPage - 1) * size)
                .Take(size)
                .ToList();

            var paged = new PagedResult<UserListApiDto>
            {
                Data = pageData,
                CurrentPage = currentPage,
                TotalPages = (int)Math.Ceiling(totalCount / (double)size),
                TotalCount = totalCount
            };

            return Result<PagedResult<UserListApiDto>>.Ok(paged);
        }

        public async Task<Result<UserDetailApiDto>> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return Result<UserDetailApiDto>.Fail("Usuario no encontrado.");

            var roles = await _userManager.GetRolesAsync(user);
            var rol = roles.FirstOrDefault()?.ToLower() ?? "";

            var cuenta = await _savingsRepo.GetPrincipalByUserIdAsync(user.Id);

            CuentaPrincipalDto? cuentaDto = null;

            if (cuenta is not null)
            {
                cuentaDto = new CuentaPrincipalDto
                {
                    NumeroCuenta = cuenta.NumeroCuenta,
                    Balance = cuenta.Balance
                };
            }

            var dto = new UserDetailApiDto
            {
                Usuario = user.UserName ?? string.Empty,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Cedula = user.Cedula,
                Correo = user.Email ?? string.Empty,
                Rol = rol,
                Estado = user.IsActive ? "activo" : "inactivo",
                CuentaPrincipal = cuentaDto
            };

            return Result<UserDetailApiDto>.Ok(dto);
        }

        public async Task<Result<string>> CreateAsync(CreateUserApiDto dto)
        {

            if (dto.Contrasena != dto.ConfirmarContrasena)
                return Result<string>.Fail("Las contraseñas no coinciden.");

            if (string.IsNullOrWhiteSpace(dto.TipoUsuario) ||
                !new[] { "administrador", "cajero", "cliente" }
                    .Contains(dto.TipoUsuario.ToLower()))
                return Result<string>.Fail("Tipo de usuario inválido.");

            var existingEmail = await _userManager.FindByEmailAsync(dto.Correo);
            if (existingEmail != null)
                return Result<string>.Fail("El correo ya está registrado.");

            var existingUserName = await _userManager.FindByNameAsync(dto.Usuario);
            if (existingUserName != null)
                return Result<string>.Fail("El nombre de usuario ya está registrado.");

            var existingCedula = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Cedula == dto.Cedula);

            if (existingCedula != null)
                return Result<string>.Fail("La cédula ya está registrada.");

            var user = new AppUser
            {
                UserName = dto.Usuario,
                Email = dto.Correo,
                Cedula = dto.Cedula,
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                IsActive = true,
                FechaCreacion = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, dto.Contrasena);

            if (!createResult.Succeeded)
                return Result<string>.Fail(createResult.Errors.Select(e => e.Description).ToList());

            var rolAsignar = dto.TipoUsuario.ToLower() switch
            {
                "administrador" => "Administrador",
                "cajero" => "Cajero",
                "cliente" => "Cliente",
                _ => null
            };

            if (rolAsignar == null)
                return Result<string>.Fail("Tipo de usuario inválido.");

            var roleResult = await _userManager.AddToRoleAsync(user, rolAsignar);

            if (!roleResult.Succeeded)
                return Result<string>.Fail(roleResult.Errors.Select(e => e.Description).ToList());

            if (dto.TipoUsuario.ToLower() == "cliente")
            {
                if (dto.MontoInicial is null || dto.MontoInicial < 0)
                    return Result<string>.Fail("Monto inicial inválido para cliente.");

                var cuenta = new SavingsAccount
                {
                    UserId = user.Id,
                    NumeroCuenta = GenerateAccountNumber(),
                    Balance = dto.MontoInicial.Value,
                    EsPrincipal = true,
                    IsActive = true,
                    FechaCreacion = DateTime.UtcNow
                };

                await _accounts.AddAsync(cuenta);
            }

            return Result<string>.Ok("Usuario creado satisfactoriamente.");
        }

        public async Task<Result<string>> CreateCommerceUserAsync(int commerceId, CreateCommerceUserApiDto dto)
        {

            if (dto.Contrasena != dto.ConfirmarContrasena)
                return Result<string>.Fail("Las contraseñas no coinciden.");

            if (string.IsNullOrWhiteSpace(dto.TipoUsuario) ||
                dto.TipoUsuario.Trim().ToLower() != "comercio")
                return Result<string>.Fail("Tipo de usuario inválido para comercio.");

            var existingEmail = await _userManager.FindByEmailAsync(dto.Correo);
            if (existingEmail != null)
                return Result<string>.Fail("El correo ya está registrado.");

            var existingUserName = await _userManager.FindByNameAsync(dto.Usuario);
            if (existingUserName != null)
                return Result<string>.Fail("El nombre de usuario ya está registrado.");

            var existingCedula = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Cedula == dto.Cedula);

            if (existingCedula != null)
                return Result<string>.Fail("La cédula ya está registrada.");


            var commerce = await _commerceRepo.GetById(commerceId);
            if (commerce == null)
                return Result<string>.Fail("Comercio no encontrado.");

            if (!commerce.IsActive)
                return Result<string>.Fail("El comercio no está activo.");

            var user = new AppUser
            {
                UserName = dto.Usuario,
                Email = dto.Correo,
                Cedula = dto.Cedula,
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                IsActive = true,
                FechaCreacion = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, dto.Contrasena);

            if (!createResult.Succeeded)
                return Result<string>.Fail(createResult.Errors.Select(e => e.Description).ToList());

            var roleResult = await _userManager.AddToRoleAsync(user, "Comercio");
            if (!roleResult.Succeeded)
                return Result<string>.Fail(roleResult.Errors.Select(e => e.Description).ToList());

            if (dto.MontoInicial < 0)
                return Result<string>.Fail("Monto inicial inválido para comercio.");

            var cuenta = new SavingsAccount
            {
                UserId = user.Id,
                NumeroCuenta = GenerateAccountNumber(),
                Balance = dto.MontoInicial,
                EsPrincipal = true,
                IsActive = true,
                FechaCreacion = DateTime.UtcNow
            };

            await _accounts.AddAsync(cuenta);

            return Result<string>.Ok("Usuario de comercio creado satisfactoriamente.");
        }

        public async Task<Result> UpdateAsync(string id, UpdateUserApiDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return Result.Fail("Usuario no encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.Correo) && !string.Equals(dto.Correo, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existingEmail = await _userManager.FindByEmailAsync(dto.Correo);
                if (existingEmail is not null && existingEmail.Id != user.Id)
                    return Result.Fail("El correo ya está registrado.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Usuario) && !string.Equals(dto.Usuario, user.UserName, StringComparison.OrdinalIgnoreCase))
            {
                var existingUserName = await _userManager.FindByNameAsync(dto.Usuario);
                if (existingUserName is not null && existingUserName.Id != user.Id)
                    return Result.Fail("El nombre de usuario ya está registrado.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Cedula) && dto.Cedula != user.Cedula)
            {
                var existingCedula = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Cedula == dto.Cedula && u.Id != user.Id);

                if (existingCedula is not null)
                    return Result.Fail("La cédula ya está registrada.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Nombre))
                user.Nombre = dto.Nombre;

            if (!string.IsNullOrWhiteSpace(dto.Apellido))
                user.Apellido = dto.Apellido;

            if (!string.IsNullOrWhiteSpace(dto.Cedula))
                user.Cedula = dto.Cedula;

            if (!string.IsNullOrWhiteSpace(dto.Correo))
                user.Email = dto.Correo;

            if (!string.IsNullOrWhiteSpace(dto.Usuario))
                user.UserName = dto.Usuario;

            var wantsPasswordChange =
                !string.IsNullOrWhiteSpace(dto.Contrasena) ||
                !string.IsNullOrWhiteSpace(dto.ConfirmarContrasena);

            if (wantsPasswordChange)
            {
                if (dto.Contrasena != dto.ConfirmarContrasena)
                    return Result.Fail("Las contraseñas no coinciden.");

                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded)
                    return Result.Fail(removeResult.Errors.Select(e => e.Description).ToList());

                var addResult = await _userManager.AddPasswordAsync(user, dto.Contrasena!);
                if (!addResult.Succeeded)
                    return Result.Fail(addResult.Errors.Select(e => e.Description).ToList());
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return Result.Fail(updateResult.Errors.Select(e => e.Description).ToList());

            if (dto.MontoAdicional.HasValue && dto.MontoAdicional.Value > 0)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var esCliente = roles.Contains("Cliente");

                if (!esCliente)
                    return Result.Fail("Solo los usuarios con rol Cliente pueden recibir monto adicional en su cuenta principal.");

                var cuentaPrincipal = await _savingsRepo.GetPrincipalByUserIdAsync(user.Id);
                if (cuentaPrincipal is null)
                    return Result.Fail("El cliente no tiene cuenta principal configurada.");

                cuentaPrincipal.Balance += dto.MontoAdicional.Value;
                await _savingsRepo.UpdateAsync(cuentaPrincipal);
            }

            return Result.Ok();
        }

        public async Task<Result> ChangeStatusAsync(string id, bool status)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return Result.Fail("Usuario no encontrado.");

            user.IsActive = status;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return Result.Fail(updateResult.Errors.Select(e => e.Description).ToList());

            return Result.Ok();
        }

        private string GenerateAccountNumber()
        {
            var random = new Random();
            return random.Next(100000000, 999999999).ToString();
        }

    }
}
