using Asp.Versioning;
using ArtemisBanking.Api.Controllers;
using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Application.Interfaces.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ArtemisBanking.Api.Controllers.v1.CreditCard
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/credit-card")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador")]
    [ApiController]
    public class CreditCardController : BaseApiController
    {
        private readonly IAdminCreditCardService _adminCreditCardService;
        private readonly IMapper _mapper;
        private const int DefaultPageSize = 20;

        public CreditCardController(IAdminCreditCardService adminCreditCardService, IMapper mapper)
        {
            _adminCreditCardService = adminCreditCardService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreditCardApiListResponseDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Obtener tarjetas de crédito",
            Description = "Retorna una lista paginada de las tarjetas de crédito registradas en el sistema con filtros opcionales por estado y cédula"
        )]
        public async Task<IActionResult> GetCreditCards(
            [FromQuery(Name = "página")] int pagina = 1,
            [FromQuery] string? estado = null,
            [FromQuery] string? cedula = null,
            CancellationToken cancellationToken = default)
        {
            if (pagina < 1) pagina = 1;

            // Normaliza el estado
            string? estadoFilter = null;
            if (!string.IsNullOrWhiteSpace(estado))
            {
                var estadoLower = estado.ToLowerInvariant();
                estadoFilter = estadoLower switch
                {
                    "activa" => "ACTIVA",
                    "cancelada" => "CANCELADA",
                    _ => null
                };
            }

            var result = await _adminCreditCardService.GetCreditCardsAsync(
                pagina,
                DefaultPageSize,
                estadoFilter,
                cedula,
                cancellationToken);

            var internalResponse = new CreditCardListResponseDTO
            {
                Items = result.Items.ToList(),
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages
            };

            var apiResponse = _mapper.Map<CreditCardApiListResponseDTO>(internalResponse);
            return Ok(apiResponse);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Asignar tarjeta de crédito",
            Description = "Asigna una nueva tarjeta de crédito a un cliente activo"
        )]
        public async Task<IActionResult> AssignCreditCard(
            [FromBody] CreditCardApiCreateRequestDTO request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized(new { message = "No se pudo identificar al administrador" });
            }

            var dto = _mapper.Map<AssignCreditCardDTO>(request);
            var result = await _adminCreditCardService.AssignCreditCardAsync(dto, adminId, cancellationToken);

            if (result.IsFailure)
            {
                // Verifica si es un error de numero de tarjeta duplicado (409 Conflict)
                if (result.GeneralError?.Contains("ya existe") == true || 
                    result.GeneralError?.Contains("duplicado") == true)
                {
                    return Conflict(new { message = result.GeneralError });
                }

                return BadRequest(new { message = result.GeneralError ?? "Error al asignar la tarjeta" });
            }

            return StatusCode(StatusCodes.Status201Created, new { message = "Tarjeta asignada correctamente" });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreditCardApiDetailResponseDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Ver detalles de una tarjeta",
            Description = "Retorna los consumos asociados a la tarjeta indicada"
        )]
        public async Task<IActionResult> GetCreditCardDetail(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var creditCard = await _adminCreditCardService.GetCreditCardByIdAsync(id, cancellationToken);

            if (creditCard == null)
            {
                return NotFound(new { message = "Tarjeta no encontrada" });
            }

            var apiResponse = _mapper.Map<CreditCardApiDetailResponseDTO>(creditCard);
            apiResponse.TarjetaId = id.ToString();
            return Ok(apiResponse);
        }


        [HttpPatch("{id}/limit")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Editar límite de una tarjeta",
            Description = "Permite modificar el límite de crédito de una tarjeta, siempre y cuando no sea inferior a la deuda actual"
        )]
        public async Task<IActionResult> UpdateCreditCardLimit(
            [FromRoute] int id,
            [FromBody] CreditCardApiUpdateLimitRequestDTO request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updateDto = _mapper.Map<UpdateCreditCardLimitDTO>(request);
            updateDto.Id = id;

            var result = await _adminCreditCardService.UpdateCreditCardLimitAsync(updateDto, cancellationToken);

            if (result.IsFailure)
            {
                if (result.GeneralError?.Contains("no encontrada") == true)
                {
                    return NotFound(new { message = result.GeneralError });
                }

                // Error de limite menor a deuda actual
                if (result.GeneralError?.Contains("menor") == true || 
                    result.GeneralError?.Contains("deuda actual") == true)
                {
                    return BadRequest(new { message = result.GeneralError });
                }

                return BadRequest(new { message = result.GeneralError ?? "Error al actualizar el límite" });
            }

            return NoContent();
        }

        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Cancelar tarjeta de crédito",
            Description = "Cancela una tarjeta si el cliente no tiene deuda pendiente en ella"
        )]
        public async Task<IActionResult> CancelCreditCard(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var result = await _adminCreditCardService.CancelCreditCardAsync(id, cancellationToken);

            if (result.IsFailure)
            {
                if (result.GeneralError?.Contains("no encontrada") == true)
                {
                    return NotFound(new { message = result.GeneralError });
                }

                // Error de deuda pendiente
                if (result.GeneralError?.Contains("deuda") == true || 
                    result.GeneralError?.Contains("saldar") == true)
                {
                    return BadRequest(new { message = result.GeneralError });
                }

                return BadRequest(new { message = result.GeneralError ?? "Error al cancelar la tarjeta" });
            }

            return NoContent();
        }
    }
}

