using ArtemisBanking.Application;
using ArtemisBanking.Application.DTOs.Common;
using ArtemisBanking.Application.DTOs.Users;
using ArtemisBanking.Application.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ArtemisBanking.Api.Controllers.v1.Users
{
    [ApiVersion("1.0")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador")]
    [Route("api/v{version:apiVersion}/users")]
    public class UsersController : BaseApiController
    {
        private readonly IUserManagementServiceApi _userService;

        public UsersController(IUserManagementServiceApi userService)
        {
            _userService = userService;
        }

        private IActionResult FromResult(Result result)
        {
            if (result.IsSuccess)
                return NoContent();

            if (result.Errors is not null && result.Errors.Any())
                return BadRequest(result.Errors);

            if (!string.IsNullOrWhiteSpace(result.GeneralError))
            {
                if (result.GeneralError.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result.GeneralError);

                return BadRequest(result.GeneralError);
            }

            return BadRequest();
        }

        private IActionResult FromResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(result.Value);

            if (result.Errors is not null && result.Errors.Any())
                return BadRequest(result.Errors);

            if (!string.IsNullOrWhiteSpace(result.GeneralError))
            {
                if (result.GeneralError.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result.GeneralError);

                return BadRequest(result.GeneralError);
            }

            return BadRequest();
        }

        private PagedResponseApi<T> MapToApiResponse<T>(PagedResult<T> result)
        {
            return new PagedResponseApi<T>
            {
                Data = result.Data,
                Paginacion = new
                {
                    paginaActual = result.CurrentPage,
                    totalPaginas = result.TotalPages,
                    totalUsuarios = result.TotalCount
                }
            };
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtener listado de usuarios",
            Description = "Obtiene un listado paginado de los usuarios registrados menos los que tienen rol comercio, ordenado del más reciente al más antiguo.")]
        [ProducesResponseType(typeof(PagedResponseApi<UserListApiDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery(Name = "rol")] string? rol)
        {
            var result = await _userService.GetUsersAsync(page, pageSize, rol);

            if (result.IsFailure)
                return FromResult(result);

            var apiResponse = MapToApiResponse(result.Value!);
            return Ok(apiResponse);
        }

        [HttpGet("commerce")]
        [SwaggerOperation(
            Summary = "Obtener listado de usuarios con rol comercio",
            Description = "Obtiene un listado paginado de los usuarios registrados con el rol comercio, ordenado del más reciente al más antiguo.")]
        [ProducesResponseType(typeof(PagedResponseApi<UserListApiDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCommerceUsers(
            [FromQuery] int? page,
            [FromQuery] int? pageSize)
        {
            var result = await _userService.GetCommerceUsersAsync(page, pageSize);

            if (result.IsFailure)
                return FromResult(result);

            var apiResponse = MapToApiResponse(result.Value!);
            return Ok(apiResponse);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Obtener detalle de usuario",
            Description = "Obtiene la información de un usuario específico, incluyendo su cuenta principal si aplica.")]
        [ProducesResponseType(typeof(UserDetailApiDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _userService.GetByIdAsync(id);
            return FromResult(result);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Crear usuario (administrador, cajero o cliente)",
            Description = "Crea un nuevo usuario del sistema. Si el tipo de usuario es cliente, se crea una cuenta de ahorro principal con el monto inicial indicado.")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateUserApiDto dto)
        {
            var result = await _userService.CreateAsync(dto);
            return FromResult(result);
        }

        [HttpPost("commerce/{commerceId:int}")]
        [SwaggerOperation(
            Summary = "Crear usuario de comercio",
            Description = "Crea un usuario con rol comercio asociado a un comercio existente. También se crea una cuenta de ahorro principal para ese usuario.")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateCommerceUser(
            int commerceId,
            [FromBody] CreateCommerceUserApiDto dto)
        {
            var result = await _userService.CreateCommerceUserAsync(commerceId, dto);
            return FromResult(result);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Actualizar usuario",
            Description = "Actualiza los datos de un usuario existente. Si el usuario es cliente y se indica un monto adicional, se suma a la cuenta principal.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserApiDto dto)
        {
            var result = await _userService.UpdateAsync(id, dto);
            return FromResult(result);
        }

        [HttpPatch("{id}/status")]
        [SwaggerOperation(
            Summary = "Cambiar estado de usuario",
            Description = "Activa o desactiva un usuario. Un usuario no puede cambiar su propio estado.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] ChangeUserStatusApiDto dto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (!string.IsNullOrWhiteSpace(currentUserId) && currentUserId == id)
            {
                return BadRequest("No puede cambiar su propio estado.");
            }

            var result = await _userService.ChangeStatusAsync(id, dto.Status);
            return FromResult(result);
        }
    }
}
