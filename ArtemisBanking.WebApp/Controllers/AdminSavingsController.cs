using System.Security.Claims;
using ArtemisBanking.Application.Dtos.SavingsAccount;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminSavingsController : Controller
    {
        private readonly IAdminSavingsAccountService _adminSavingsAccountService;
        private readonly IMapper _mapper;
        private const int PageSize = 20;

        public AdminSavingsController(
            IAdminSavingsAccountService adminSavingsAccountService,
            IMapper mapper)
        {
            _adminSavingsAccountService = adminSavingsAccountService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int page = 1,
            string? estadoFilter = null,
            string? tipoFilter = null,
            string? cedulaFilter = null,
            CancellationToken cancellationToken = default)
        {
            var effectiveEstado = string.IsNullOrWhiteSpace(estadoFilter)
                ? (string.IsNullOrWhiteSpace(cedulaFilter) ? "ACTIVAS" : "TODAS")
                : estadoFilter;
            var effectiveTipo = string.IsNullOrWhiteSpace(tipoFilter) ? "TODAS" : tipoFilter;

            var result = await _adminSavingsAccountService.GetSavingsAccountsAsync(
                page,
                PageSize,
                effectiveEstado,
                effectiveTipo,
                cedulaFilter,
                cancellationToken);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.EstadoFilter = effectiveEstado;
            ViewBag.TipoFilter = effectiveTipo;
            ViewBag.CedulaFilter = cedulaFilter;

            var viewModels = _mapper.Map<List<SavingsAccountListItemViewModel>>(result.Items);
            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> AssignStep1(string? cedulaFilter = null, CancellationToken cancellationToken = default)
        {
            var clients = await _adminSavingsAccountService.GetEligibleClientsAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(cedulaFilter))
            {
                var normalized = NormalizeCedula(cedulaFilter);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    clients = clients
                        .Where(c => NormalizeCedula(c.Cedula).Contains(normalized))
                        .ToList();
                }
            }

            ViewBag.CedulaFilter = cedulaFilter;
            var viewModels = _mapper.Map<List<ClientForSavingsAccountViewModel>>(clients);
            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignStep1(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["ErrorMessage"] = "Debe seleccionar un cliente";
                return RedirectToAction(nameof(AssignStep1));
            }

            return RedirectToAction(nameof(AssignStep2), new { userId });
        }

        [HttpGet]
        public async Task<IActionResult> AssignStep2(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["ErrorMessage"] = "Cliente no válido";
                return RedirectToAction(nameof(AssignStep1));
            }

            var clients = await _adminSavingsAccountService.GetEligibleClientsAsync(cancellationToken);
            var client = clients.FirstOrDefault(c => c.UserId == userId);

            if (client == null)
            {
                TempData["ErrorMessage"] = "Cliente no encontrado o no elegible";
                return RedirectToAction(nameof(AssignStep1));
            }

            ViewBag.Client = _mapper.Map<ClientForSavingsAccountViewModel>(client);
            var model = new AssignSavingsAccountViewModel
            {
                UserId = userId,
                BalanceInicial = 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStep2(AssignSavingsAccountViewModel model, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(model.UserId))
            {
                TempData["ErrorMessage"] = "Cliente no válido";
                return RedirectToAction(nameof(AssignStep1));
            }

            var clients = await _adminSavingsAccountService.GetEligibleClientsAsync(cancellationToken);
            var client = clients.FirstOrDefault(c => c.UserId == model.UserId);
            if (client == null)
            {
                TempData["ErrorMessage"] = "Cliente no encontrado o no elegible";
                return RedirectToAction(nameof(AssignStep1));
            }

            ViewBag.Client = _mapper.Map<ClientForSavingsAccountViewModel>(client);

            // Asegurar que el balance inicial sea al menos 0
            if (model.BalanceInicial < 0)
            {
                ModelState.AddModelError(nameof(model.BalanceInicial), "El balance inicial no puede ser negativo");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al administrador";
                return RedirectToAction(nameof(Index));
            }

            var dto = _mapper.Map<AssignSavingsAccountDTO>(model);
            var result = await _adminSavingsAccountService.AssignSecondaryAccountAsync(dto, adminId, cancellationToken);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.GeneralError ?? "No se pudo asignar la cuenta");
                return View(model);
            }

            TempData["SuccessMessage"] = "Cuenta secundaria asignada correctamente";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken = default)
        {
            var detail = await _adminSavingsAccountService.GetAccountDetailAsync(id, cancellationToken);
            if (detail == null)
            {
                TempData["ErrorMessage"] = "Cuenta no encontrada";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AccountId = id;
            return View(detail);
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken = default)
        {
            var detail = await _adminSavingsAccountService.GetAccountDetailAsync(id, cancellationToken);
            if (detail == null)
            {
                TempData["ErrorMessage"] = "Cuenta no encontrada";
                return RedirectToAction(nameof(Index));
            }

            if (detail.EsPrincipal)
            {
                TempData["ErrorMessage"] = "Las cuentas principales no pueden cancelarse";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AccountId = id;
            return View(detail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Cancel")]
        public async Task<IActionResult> CancelConfirmed(int id, CancellationToken cancellationToken = default)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al administrador";
                return RedirectToAction(nameof(Index));
            }

            var result = await _adminSavingsAccountService.CancelSecondaryAccountAsync(id, adminId, cancellationToken);
            if (result.IsFailure)
            {
                TempData["ErrorMessage"] = result.GeneralError ?? "No se pudo cancelar la cuenta";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Cuenta secundaria cancelada correctamente";
            return RedirectToAction(nameof(Index));
        }

        private static string NormalizeCedula(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return new string(value.Where(char.IsDigit).ToArray());
        }
    }
}

