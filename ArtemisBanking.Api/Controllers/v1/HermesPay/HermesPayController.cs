using ArtemisBanking.Application.DTOs.Common;
using ArtemisBanking.Application.DTOs.Hermes;
using ArtemisBanking.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArtemisBanking.Api.Controllers
{
    [ApiController]
    [Route("pay")]
    [Authorize(Roles = "Administrador,Comercio")]
    public class HermesPayController : ControllerBase
    {
        private readonly IHermesPayService _hermes;

        public HermesPayController(IHermesPayService hermes)
        {
            _hermes = hermes;
        }
        [HttpGet("get-transactions/{commerceId:int?}")]
        [SwaggerOperation(
            Summary = "Obtener transacciones de un comercio",
            Description = @"Obtiene un listado paginado de las transacciones registradas para un comercio.
            Si el usuario autenticado tiene rol Comercio, el identificador del comercio se obtiene del token JWT e ignora el parámetro commerceId de la URL.
            Si el usuario tiene rol Administrador, debe enviar explícitamente el commerceId en la ruta.")]
        [ProducesResponseType(typeof(PagedResponseApi<CommerceTransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTransactions(
            int? commerceId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            int finalCommerceId;

            if (User.IsInRole("Comercio"))
            {
                var claim = User.FindFirst("commerceId")?.Value;
                if (!int.TryParse(claim, out finalCommerceId))
                    return Forbid();
            }
            else
            {
                if (!commerceId.HasValue)
                    return BadRequest("Debe enviar commerceId.");

                finalCommerceId = commerceId.Value;
            }

            var result = await _hermes.GetTransactionsAsync(finalCommerceId, page, pageSize);

            if (result.IsFailure)
                return BadRequest(result.GeneralError ?? "Error.");

            var paged = result.Value!;

            var response = new PagedResponseApi<CommerceTransactionDto>
            {
                Data = paged.Data,
                Paginacion = new
                {
                    paginaActual = paged.CurrentPage,
                    paginasTotales = paged.TotalPages,
                    totalElementos = paged.TotalCount
                }
            };

            return Ok(response);
        }

        [HttpPost("process-payment/{commerceId:int?}")]
        [SwaggerOperation(
            Summary = "Procesar pago de un comercio",
            Description = @"Recibe los datos de un pago para ser procesado.
            Si el usuario autenticado tiene rol Comercio, el identificador del comercio se obtiene del token JWT e ignora el parámetro commerceId de la URL.
            Si el usuario tiene rol Administrador, debe enviar explícitamente el commerceId en la ruta.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ProcessPayment(
            int? commerceId,
            [FromBody] ProcessPaymentRequestDto request)
        {
            int finalCommerceId;

            if (User.IsInRole("Comercio"))
            {
                var claim = User.FindFirst("commerceId")?.Value;
                if (!int.TryParse(claim, out finalCommerceId))
                    return Forbid();
            }
            else
            {
                if (!commerceId.HasValue)
                    return BadRequest("Debe enviar commerceId.");

                finalCommerceId = commerceId.Value;
            }

            var result = await _hermes.ProcessPaymentAsync(finalCommerceId, request);

            if (result.IsFailure)
                return BadRequest(result.GeneralError ?? "Error.");

            return NoContent();
        }
    }
}
