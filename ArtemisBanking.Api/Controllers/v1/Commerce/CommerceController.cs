using ArtemisBanking.Application;
using ArtemisBanking.Application.DTOs.Commerce;
using ArtemisBanking.Application.DTOs.Common;
using ArtemisBanking.Application.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArtemisBanking.Api.Controllers.v1.Commerce
{
    [ApiVersion("1.0")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador")]
    public class CommerceController : BaseApiController
    {
        private readonly ICommerceServiceApi _commerceService;

        public CommerceController(ICommerceServiceApi commerceService)
        {
            _commerceService = commerceService;
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

        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtener todos los comercios (paginado)",
            Description = "Devuelve un listado paginado de comercios activos, ordenados del más reciente al más antiguo.")]
        [ProducesResponseType(typeof(PagedResult<CommerceListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var result = await _commerceService.GetAllAsync(page, pageSize);
            return FromResult(result);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obtener comercio por ID",
            Description = "Devuelve los detalles de un comercio segn su identificador.")]
        [ProducesResponseType(typeof(CommerceDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _commerceService.GetByIdAsync(id);
            return FromResult(result);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Crear nuevo comercio",
            Description = "Crea un nuevo comercio en el sistema.")]
        [ProducesResponseType(typeof(CommerceDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CommerceCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _commerceService.CreateAsync(dto);

            if (result.IsFailure)
                return FromResult(result);

            var getResult = await _commerceService.GetByIdAsync(result.Value!);
            if (getResult.IsFailure)
                return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, getResult.Value);
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(
            Summary = "Actualizar comercio existente",
            Description = "Actualiza los datos de un comercio existente.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CommerceCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _commerceService.UpdateAsync(id, dto);
            if (result.IsFailure)
                return FromResult(result);

            return NoContent();
        }

        [HttpPatch("{id:int}")]
        [SwaggerOperation(
            Summary = "Cambiar estado de un comercio",
            Description = "Activa o desactiva un comercio.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] CommerceChangeStatusDto dto)
        {
            var result = await _commerceService.ChangeStatusAsync(id, dto.Status);
            if (result.IsFailure)
                return FromResult(result);

            return NoContent();
        }
    }
}
