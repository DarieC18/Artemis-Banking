using Asp.Versioning;
using ArtemisBanking.Api.Controllers;
using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Dtos.SavingsAccount;
using ArtemisBanking.Application.Interfaces.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ArtemisBanking.Api.Controllers.v1.SavingsAccount
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/savings-account")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrador")]
    [ApiController]
    public class SavingsAccountController : BaseApiController
    {
        private readonly IAdminSavingsAccountService _adminSavingsAccountService;
        private readonly IMapper _mapper;
        private const int DefaultPageSize = 20;

        public SavingsAccountController(IAdminSavingsAccountService adminSavingsAccountService, IMapper mapper)
        {
            _adminSavingsAccountService = adminSavingsAccountService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SavingsAccountApiListResponseDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Obtener cuentas de ahorro",
            Description = "Retorna una lista paginada de cuentas de ahorro con filtros opcionales por estado, tipo y cédula"
        )]
        public async Task<IActionResult> GetSavingsAccounts(
            [FromQuery(Name = "página")] int pagina = 1,
            [FromQuery] string? estado = null,
            [FromQuery] string? tipo = null,
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
                    "activo" => "ACTIVAS",
                    "cancelado" => "CANCELADAS",
                    _ => null
                };
            }

            // Normaliza el tipo
            string? tipoFilter = null;
            if (!string.IsNullOrWhiteSpace(tipo))
            {
                var tipoLower = tipo.ToLowerInvariant();
                tipoFilter = tipoLower switch
                {
                    "principal" => "PRINCIPAL",
                    "secundaria" => "SECUNDARIA",
                    _ => null
                };
            }

            var result = await _adminSavingsAccountService.GetSavingsAccountsAsync(
                pagina,
                DefaultPageSize,
                estadoFilter,
                tipoFilter,
                cedula,
                cancellationToken);

            var apiResponse = new SavingsAccountApiListResponseDTO
            {
                Data = _mapper.Map<List<SavingsAccountApiListItemDTO>>(result.Items),
                CurrentPage = result.PageNumber,
                TotalPages = result.TotalPages
            };

            return Ok(apiResponse);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Crear cuenta de ahorro secundaria",
            Description = "Crea una nueva cuenta de ahorro secundaria para un cliente"
        )]
        public async Task<IActionResult> CreateSavingsAccount(
            [FromBody] SavingsAccountApiCreateRequestDTO request,
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

            var dto = _mapper.Map<AssignSavingsAccountDTO>(request);
            var result = await _adminSavingsAccountService.AssignSecondaryAccountAsync(dto, adminId, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.GeneralError ?? "Error al crear la cuenta de ahorro" });
            }

            return StatusCode(StatusCodes.Status201Created, new { message = "Cuenta de ahorro creada exitosamente" });
        }

        [HttpGet("{accountNumber}/transactions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SavingsAccountApiTransactionsResponseDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Obtener transacciones de una cuenta",
            Description = "Retorna todas las transacciones asociadas a una cuenta de ahorro"
        )]
        public async Task<IActionResult> GetAccountTransactions(
            [FromRoute] string accountNumber,
            CancellationToken cancellationToken = default)
        {
            // Busca la cuenta por numero para obtener su ID
            var allAccounts = await _adminSavingsAccountService.GetSavingsAccountsAsync(
                1, 
                1000, 
                null, 
                null, 
                null, 
                cancellationToken);

            var account = allAccounts.Items.FirstOrDefault(a => a.NumeroCuenta == accountNumber);

            if (account == null)
            {
                return NotFound(new { message = "Cuenta no encontrada" });
            }

            // Obtiene el detalle completo con transacciones usando el ID
            var accountDetail = await _adminSavingsAccountService.GetAccountDetailAsync(account.Id, cancellationToken);

            if (accountDetail == null)
            {
                return NotFound(new { message = "Cuenta no encontrada" });
            }

            var apiResponse = _mapper.Map<SavingsAccountApiTransactionsResponseDTO>(accountDetail);
            return Ok(apiResponse);
        }
    }
}

