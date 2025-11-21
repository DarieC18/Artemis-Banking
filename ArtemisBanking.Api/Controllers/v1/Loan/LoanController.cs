using ArtemisBanking.Application.Dtos.Loan;
using ArtemisBanking.Application.Interfaces.Services;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ArtemisBanking.Api.Controllers.v1.Loan
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/loan")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador")]
    [ApiController]
    public class LoanController : BaseApiController
    {
        private readonly IAdminLoanService _adminLoanService;
        private readonly IMapper _mapper;
        private const int DefaultPageSize = 20;

        public LoanController(IAdminLoanService adminLoanService, IMapper mapper)
        {
            _adminLoanService = adminLoanService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoanApiListResponseDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Obtener listado de préstamos",
            Description = "Retorna un listado paginado de préstamos con filtros opcionales por estado y cédula del cliente"
        )]
        public async Task<IActionResult> GetLoans(
            [FromQuery(Name = "página")] int pagina = 1,
            [FromQuery] string? estado = null,
            [FromQuery] string? cedula = null,
            CancellationToken cancellationToken = default)
        {
            if (pagina < 1) pagina = 1;

            // Normaliza el estado
            string? statusFilter = null;
            if (!string.IsNullOrWhiteSpace(estado))
            {
                var estadoLower = estado.ToLowerInvariant();
                statusFilter = estadoLower switch
                {
                    "activos" => "Activos",
                    "completados" => "Completados",
                    _ => null
                };
            }

            var result = await _adminLoanService.GetLoansAsync(
                pagina,
                DefaultPageSize,
                statusFilter,
                cedula,
                cancellationToken);

            var internalResponse = new LoanListResponseDTO
            {
                Items = result.Items.ToList(),
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages
            };

            var apiResponse = _mapper.Map<LoanApiListResponseDTO>(internalResponse);
            return Ok(apiResponse);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Asignar préstamo a cliente",
            Description = "Crea un nuevo préstamo para un cliente, genera la tabla de amortización y acredita el monto a la cuenta principal"
        )]
        public async Task<IActionResult> AssignLoan(
            [FromBody] LoanApiCreateRequestDTO request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Valida que el plazo sea multiplo de 6 y este en el rango permitido
            if (request.PlazoMeses % 6 != 0 || request.PlazoMeses < 6 || request.PlazoMeses > 60)
            {
                return BadRequest(new { message = "El plazo debe ser un múltiplo de 6 meses entre 6 y 60" });
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized(new { message = "No se pudo identificar al administrador" });
            }

            var dto = _mapper.Map<AssignLoanDTO>(request);
            var result = await _adminLoanService.AssignLoanAsync(dto, adminId, ignoreRisk: false, cancellationToken);

            if (result.IsFailure)
            {
                // Verifica si es un error de cliente de alto riesgo (409 Conflict)
                if (result.GeneralError?.Contains("alto riesgo") == true)
                {
                    return Conflict(new { message = result.GeneralError });
                }

                // Verifica si es un error de prestamo activo (400 Bad Request)
                if (result.GeneralError?.Contains("ya tiene un préstamo activo") == true)
                {
                    return BadRequest(new { message = result.GeneralError });
                }

                return BadRequest(new { message = result.GeneralError ?? "Error al asignar el préstamo" });
            }

            return StatusCode(StatusCodes.Status201Created, new { message = "Préstamo creado exitosamente" });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoanApiDetailResponseDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Obtener detalle de préstamo",
            Description = "Retorna el detalle completo del préstamo incluyendo la tabla de amortización"
        )]
        public async Task<IActionResult> GetLoanDetail(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var loan = await _adminLoanService.GetLoanByIdAsync(id, cancellationToken);

            if (loan == null)
            {
                return NotFound(new { message = "Préstamo no encontrado" });
            }

            var apiResponse = _mapper.Map<LoanApiDetailResponseDTO>(loan);
            apiResponse.PrestamoId = id.ToString();
            return Ok(apiResponse);
        }

        [HttpPatch("{id}/rate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Editar tasa de interés de préstamo",
            Description = "Actualiza la tasa de interés del préstamo y recalcula las cuotas futuras pendientes"
        )]
        public async Task<IActionResult> UpdateLoanRate(
            [FromRoute] int id,
            [FromBody] LoanApiUpdateRateRequestDTO request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updateDto = _mapper.Map<UpdateLoanDTO>(request);
            updateDto.Id = id;

            var result = await _adminLoanService.UpdateLoanAsync(updateDto, cancellationToken);

            if (result.IsFailure)
            {
                if (result.GeneralError?.Contains("no encontrado") == true)
                {
                    return NotFound(new { message = result.GeneralError });
                }

                return BadRequest(new { message = result.GeneralError ?? "Error al actualizar la tasa de interés" });
            }

            return NoContent();
        }
    }
}

