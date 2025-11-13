using Asp.Versioning;
using ArtemisBanking.Application.DTOs.Account;
using ArtemisBanking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ArtemisBanking.Api.Controllers.v1.Login
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/account")]
    public class LoginController : BaseApiController
    {
        private readonly IAccountServiceApi _accountServiceApi;

        public LoginController(IAccountServiceApi accountServiceApi)
        {
            _accountServiceApi = accountServiceApi;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JwtResponseDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Autenticar usuario", Description = "Permite obtener un token JWT para acceder a la API")] 
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _accountServiceApi.LoginAsync(dto);

            if (!result.Success)
            {
                return result.StatusCode switch
                {
                    400 => BadRequest(result.Message),
                    401 => Unauthorized(result.Message),
                    403 => Forbid(),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
                };
            }

            return Ok(result.Data);
        }

        [HttpPost("confirm")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador,Comercio")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Confirmar cuenta", Description = "Activa un usuario utilizando un token de confirmación")]
        public async Task<IActionResult> ConfirmAccount([FromBody] ConfirmAccountApiRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _accountServiceApi.ConfirmAccountAsync(dto);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return NoContent();
        }

        [HttpPost("get-reset-token")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador,Comercio")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Generar token de reseteo", Description = "Genera un token de reseteo de contraseña y lo envía por correo")]
        public async Task<IActionResult> GetResetToken([FromBody] ForgotPasswordApiRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _accountServiceApi.GenerateResetTokenAsync(dto);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return NoContent();
        }

        [HttpPost("reset-password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador,Comercio")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Restablecer contraseña", Description = "Permite restablecer la contraseña de un usuario utilizando un token")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordByIdDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _accountServiceApi.ResetPasswordByIdAsync(dto);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return NoContent();
        }
    }
}
