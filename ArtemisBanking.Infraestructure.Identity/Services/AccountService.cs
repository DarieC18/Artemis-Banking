using ArtemisBanking.Application.DTOs.Account;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Infraestructure.Identity.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace ArtemisBanking.Infraestructure.Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public AccountService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailService emailService,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<AuthenticationResultDTO> AuthenticateAsync(LoginDTO loginDto)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(loginDto.UserName);

                if (user == null)
                {
                    return new AuthenticationResultDTO
                    {
                        Success = false,
                        Message = "Datos de acceso incorrectos"
                    };
                }

                if (!user.IsActive)
                {
                    if (string.IsNullOrWhiteSpace(user.ResetPasswordToken) || user.ResetPasswordTokenExpiry == null || user.ResetPasswordTokenExpiry < DateTime.Now)
                    {
                        user.ResetPasswordToken = Guid.NewGuid().ToString();
                        user.ResetPasswordTokenExpiry = DateTime.Now.AddHours(24);
                        await _userManager.UpdateAsync(user);
                    }

                    if (!string.IsNullOrWhiteSpace(user.Email))
                    {
                        await _emailService.SendAccountConfirmationEmailAsync(user.Email, user.UserName, user.ResetPasswordToken!);
                    }

                    return new AuthenticationResultDTO
                    {
                        Success = false,
                        Message = "Su cuenta está inactiva. Hemos reenviado el enlace de activación a su correo"
                    };
                }

                var result = await _signInManager.PasswordSignInAsync(loginDto.UserName, loginDto.Password, false, false);

                if (result.Succeeded)
                {
                    var userDto = _mapper.Map<UserDTO>(user);
                    userDto.Roles = await _userManager.GetRolesAsync(user);

                    return new AuthenticationResultDTO
                    {
                        Success = true,
                        Message = "Autenticación exitosa",
                        User = userDto
                    };
                }

                return new AuthenticationResultDTO
                {
                    Success = false,
                    Message = "Datos de acceso incorrectos"
                };
            }
            catch (Exception ex)
            {
                return new AuthenticationResultDTO
                {
                    Success = false,
                    Message = $"Error en autenticación: {ex.Message}"
                };
            }
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<ServiceResultDTO> RequestPasswordResetAsync(ForgotPasswordDTO forgotPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(forgotPasswordDto.UserName);

                if (user == null)
                {
                    return new ServiceResultDTO
                    {
                        Success = true,
                        Message = "Si el usuario existe, se ha enviado un correo con instrucciones para restablecer la contraseña"
                    };
                }

                user.IsActive = false;

                var token = Guid.NewGuid().ToString();
                user.ResetPasswordToken = token;
                user.ResetPasswordTokenExpiry = DateTime.Now.AddHours(24);

                await _userManager.UpdateAsync(user);

                await _emailService.SendPasswordResetEmailAsync(user.Email, user.UserName, token);

                return new ServiceResultDTO
                {
                    Success = true,
                    Message = "Si el usuario existe, se ha enviado un correo con instrucciones para restablecer la contraseña"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResultDTO
                {
                    Success = false,
                    Message = $"Error al procesar solicitud: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResultDTO> ResetPasswordAsync(ResetPasswordDTO resetPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(resetPasswordDto.UserName);

                if (user == null)
                {
                    return new ServiceResultDTO
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                if (string.IsNullOrEmpty(user.ResetPasswordToken) || user.ResetPasswordToken != resetPasswordDto.Token)
                {
                    return new ServiceResultDTO
                    {
                        Success = false,
                        Message = "Token inválido o expirado"
                    };
                }

                if (user.ResetPasswordTokenExpiry == null || user.ResetPasswordTokenExpiry < DateTime.Now)
                {
                    return new ServiceResultDTO
                    {
                        Success = false,
                        Message = "Token inválido o expirado"
                    };
                }

                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    return new ServiceResultDTO
                    {
                        Success = false,
                        Message = "Error al cambiar contraseña"
                    };
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(user, resetPasswordDto.Password);
                if (!addPasswordResult.Succeeded)
                {
                    var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                    return new ServiceResultDTO
                    {
                        Success = false,
                        Message = $"Error al establecer nueva contraseña: {errors}"
                    };
                }

                user.ResetPasswordToken = null;
                user.ResetPasswordTokenExpiry = null;
                user.IsActive = true;

                await _userManager.UpdateAsync(user);

                return new ServiceResultDTO
                {
                    Success = true,
                    Message = "Contraseña restablecida exitosamente. Tu cuenta ha sido reactivada"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResultDTO
                {
                    Success = false,
                    Message = $"Error al restablecer contraseña: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResultDTO> ConfirmAccountAsync(string token)
        {
            try
            {
                var user = _userManager.Users.FirstOrDefault(u => u.ResetPasswordToken == token);

                if (user == null)
                {
                    return new ServiceResultDTO
                    {
                        Success = false,
                        Message = "Token de confirmación inválido"
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
                    Message = "Cuenta confirmada exitosamente. Ya puedes iniciar sesión"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResultDTO
                {
                    Success = false,
                    Message = $"Error al confirmar cuenta: {ex.Message}"
                };
            }
        }

        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<string>();
            }
            return await _userManager.GetRolesAsync(user);
        }
    }
}
