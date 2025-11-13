using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArtemisBanking.Application.DTOs.Account;
using ArtemisBanking.Application.DTOs.Email;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Settings;
using ArtemisBanking.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ArtemisBanking.Infraestructure.Identity.Services
{
    public class AccountServiceApi : IAccountServiceApi
    {
        private static readonly string[] AllowedRoles = ["Administrador", "Comercio"];

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly JwtSettings _jwtSettings;
        private readonly TimeProvider _timeProvider;

        public AccountServiceApi(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailService emailService,
            IOptions<JwtSettings> jwtSettings,
            TimeProvider timeProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _jwtSettings = jwtSettings.Value;
            _timeProvider = timeProvider;
        }

        public async Task<ServiceResultWithDataDTO<JwtResponseDTO>> LoginAsync(LoginDTO loginDto)
        {
            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.UserName) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return new ServiceResultWithDataDTO<JwtResponseDTO>
                {
                    Success = false,
                    Message = "Credenciales inválidas",
                    StatusCode = 400
                };
            }

            var user = await _userManager.FindByNameAsync(loginDto.UserName);

            if (user == null)
            {
                return new ServiceResultWithDataDTO<JwtResponseDTO>
                {
                    Success = false,
                    Message = "Usuario o contraseña incorrectos",
                    StatusCode = 401
                };
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Any(role => AllowedRoles.Contains(role)))
            {
                return new ServiceResultWithDataDTO<JwtResponseDTO>
                {
                    Success = false,
                    Message = "El usuario no tiene permisos para acceder a la API",
                    StatusCode = 403
                };
            }

            if (!user.IsActive)
            {
                return new ServiceResultWithDataDTO<JwtResponseDTO>
                {
                    Success = false,
                    Message = "La cuenta está inactiva. Debe ser confirmada antes de iniciar sesión",
                    StatusCode = 401
                };
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!signInResult.Succeeded)
            {
                return new ServiceResultWithDataDTO<JwtResponseDTO>
                {
                    Success = false,
                    Message = "Usuario o contraseña incorrectos",
                    StatusCode = 401
                };
            }

            var token = BuildToken(user, userRoles);

            return new ServiceResultWithDataDTO<JwtResponseDTO>
            {
                Success = true,
                Message = "Autenticación exitosa",
                Data = new JwtResponseDTO { Jwt = token },
                StatusCode = 200
            };
        }

        public async Task<ServiceResultDTO> ConfirmAccountAsync(ConfirmAccountApiRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Token))
            {
                return new ServiceResultDTO
                {
                    Success = false,
                    Message = "Token inválido"
                };
            }

            var user = _userManager.Users.FirstOrDefault(u => u.ResetPasswordToken == request.Token);
            if (user == null)
            {
                return new ServiceResultDTO
                {
                    Success = false,
                    Message = "Token inválido o expirado"
                };
            }

            user.IsActive = true;
            user.EmailConfirmed = true;
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;

            await _userManager.UpdateAsync(user);

            return new ServiceResultDTO
            {
                Success = true,
                Message = "Cuenta confirmada exitosamente"
            };
        }

        public async Task<ServiceResultDTO> GenerateResetTokenAsync(ForgotPasswordApiRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserName))
            {
                return new ServiceResultDTO
                {
                    Success = false,
                    Message = "El nombre de usuario es requerido"
                };
            }

            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
            {
                return new ServiceResultDTO
                {
                    Success = false,
                    Message = "Usuario no encontrado"
                };
            }

            user.IsActive = false;
            user.ResetPasswordToken = Guid.NewGuid().ToString();
            user.ResetPasswordTokenExpiry = _timeProvider.GetUtcNow().DateTime.AddHours(24);

            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = "Token para restablecer contraseña - ArtemisBanking",
                    Body = $"Hola {user.UserName}, tu token para restablecer la contraseña es: {user.ResetPasswordToken}"
                });
            }

            return new ServiceResultDTO
            {
                Success = true,
                Message = "Se generó el token de restablecimiento y se envió al correo registrado"
            };
        }

        public async Task<ServiceResultDTO> ResetPasswordByIdAsync(ResetPasswordByIdDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Token))
            {
                return new ServiceResultDTO
                {
                    Success = false,
                    Message = "Datos de reseteo inválidos"
                };
            }

            if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
            {
                return new ServiceResultDTO
                {
                    Success = false,
                    Message = "Las contraseñas no coinciden"
                };
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return new ServiceResultDTO
                {
                    Success = false,
                    Message = "Usuario no encontrado"
                };
            }

            if (string.IsNullOrWhiteSpace(user.ResetPasswordToken) || user.ResetPasswordToken != request.Token)
            {
                return new ServiceResultDTO
                {
                    Success = false,
                    Message = "Token inválido"
                };
            }

            if (user.ResetPasswordTokenExpiry == null || user.ResetPasswordTokenExpiry < _timeProvider.GetUtcNow().DateTime)
            {
                return new ServiceResultDTO
                {
                    Success = false,
                    Message = "Token expirado"
                };
            }

            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                return new ServiceResultDTO
                {
                    Success = false,
                    Message = "No se pudo cambiar la contraseña"
                };
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, request.Password);
            if (!addPasswordResult.Succeeded)
            {
                var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                return new ServiceResultDTO
                {
                    Success = false,
                    Message = $"Error al establecer la contraseña: {errors}"
                };
            }

            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;
            user.IsActive = true;

            await _userManager.UpdateAsync(user);

            return new ServiceResultDTO
            {
                Success = true,
                Message = "Contraseña restablecida correctamente"
            };
        }

        private string BuildToken(AppUser user, IList<string> roles)
        {
            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            var expiration = utcNow.AddMinutes(_jwtSettings.DurationInMinutes > 0 ? _jwtSettings.DurationInMinutes : 60);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                new(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: utcNow,
                expires: expiration,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
