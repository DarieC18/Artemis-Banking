using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBanking.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminCreditCardsController : Controller
    {
        private readonly IAdminCreditCardService _adminCreditCardService;
        private readonly IMapper _mapper;

        public AdminCreditCardsController(IAdminCreditCardService adminCreditCardService, IMapper mapper)
        {
            _adminCreditCardService = adminCreditCardService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int page = 1, 
            string? estadoFilter = null, 
            string? cedulaFilter = null, 
            CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            const int pageSize = 20;

            var result = await _adminCreditCardService.GetCreditCardsAsync(
                page, pageSize, estadoFilter, cedulaFilter, cancellationToken);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.EstadoFilter = estadoFilter ?? "ACTIVA";
            ViewBag.CedulaFilter = cedulaFilter;

            var viewModels = _mapper.Map<List<CreditCardListItemViewModel>>(result.Items);
            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> AssignStep1(string? cedulaFilter = null, CancellationToken cancellationToken = default)
        {
            var averageDebt = await _adminCreditCardService.GetAverageDebtAsync(cancellationToken);
            var clients = await _adminCreditCardService.GetEligibleClientsAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(cedulaFilter))
            {
                clients = clients.Where(c => c.Cedula.Contains(cedulaFilter)).ToList();
            }

            ViewBag.AverageDebt = averageDebt;
            ViewBag.CedulaFilter = cedulaFilter;
            var viewModels = _mapper.Map<List<ClientForCreditCardViewModel>>(clients);
            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignStep1(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Debe seleccionar un cliente";
                return RedirectToAction(nameof(AssignStep1));
            }

            return RedirectToAction(nameof(AssignStep2), new { userId });
        }

        [HttpGet]
        public async Task<IActionResult> AssignStep2(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Cliente no válido";
                return RedirectToAction(nameof(AssignStep1));
            }

            var clients = await _adminCreditCardService.GetEligibleClientsAsync(cancellationToken);
            var client = clients.FirstOrDefault(c => c.UserId == userId);

            if (client == null)
            {
                TempData["ErrorMessage"] = "Cliente no encontrado o no elegible";
                return RedirectToAction(nameof(AssignStep1));
            }

            ViewBag.Client = _mapper.Map<ClientForCreditCardViewModel>(client);

            return View(new AssignCreditCardViewModel { UserId = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStep2(AssignCreditCardViewModel model, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var clients = await _adminCreditCardService.GetEligibleClientsAsync(cancellationToken);
                var client = clients.FirstOrDefault(c => c.UserId == model.UserId);
                if (client != null)
                {
                    ViewBag.Client = _mapper.Map<ClientForCreditCardViewModel>(client);
                }
                return View(model);
            }

            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentAdminId))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al administrador";
                return RedirectToAction(nameof(Index));
            }

            var dto = _mapper.Map<AssignCreditCardDTO>(model);
            var result = await _adminCreditCardService.AssignCreditCardAsync(dto, currentAdminId, cancellationToken);

            if (result.IsFailure)
            {
                ModelState.AddModelError("", result.GeneralError ?? "Error al asignar la tarjeta");
                var clients = await _adminCreditCardService.GetEligibleClientsAsync(cancellationToken);
                var client = clients.FirstOrDefault(c => c.UserId == model.UserId);
                if (client != null)
                {
                    ViewBag.Client = _mapper.Map<ClientForCreditCardViewModel>(client);
                }
                return View(model);
            }

            TempData["SuccessMessage"] = "Tarjeta asignada exitosamente";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken = default)
        {
            var card = await _adminCreditCardService.GetCreditCardByIdAsync(id, cancellationToken);
            if (card == null)
            {
                TempData["ErrorMessage"] = "Tarjeta no encontrada";
                return RedirectToAction(nameof(Index));
            }

            return View(card);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
        {
            var card = await _adminCreditCardService.GetCreditCardByIdAsync(id, cancellationToken);
            if (card == null)
            {
                TempData["ErrorMessage"] = "Tarjeta no encontrada";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new UpdateCreditCardLimitViewModel
            {
                Id = id,
                LimiteCredito = card.LimiteCredito
            };

            ViewBag.Ultimos4Digitos = card.Ultimos4Digitos;
            ViewBag.DeudaActual = card.DeudaActual;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateCreditCardLimitViewModel model, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var card = await _adminCreditCardService.GetCreditCardByIdAsync(model.Id, cancellationToken);
                if (card != null)
                {
                    ViewBag.Ultimos4Digitos = card.Ultimos4Digitos;
                    ViewBag.DeudaActual = card.DeudaActual;
                }
                return View(model);
            }

            var dto = _mapper.Map<UpdateCreditCardLimitDTO>(model);
            var result = await _adminCreditCardService.UpdateCreditCardLimitAsync(dto, cancellationToken);

            if (result.IsFailure)
            {
                ModelState.AddModelError("", result.GeneralError ?? "Error al actualizar el límite");
                var card = await _adminCreditCardService.GetCreditCardByIdAsync(model.Id, cancellationToken);
                if (card != null)
                {
                    ViewBag.Ultimos4Digitos = card.Ultimos4Digitos;
                    ViewBag.DeudaActual = card.DeudaActual;
                }
                return View(model);
            }

            TempData["SuccessMessage"] = "Límite actualizado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken = default)
        {
            var card = await _adminCreditCardService.GetCreditCardByIdAsync(id, cancellationToken);
            if (card == null)
            {
                TempData["ErrorMessage"] = "Tarjeta no encontrada";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Ultimos4Digitos = card.Ultimos4Digitos;
            ViewBag.Id = id;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Cancel")]
        public async Task<IActionResult> CancelConfirmed(int id, CancellationToken cancellationToken = default)
        {
            var result = await _adminCreditCardService.CancelCreditCardAsync(id, cancellationToken);

            if (result.IsFailure)
            {
                TempData["ErrorMessage"] = result.GeneralError ?? "Error al cancelar la tarjeta";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Tarjeta cancelada exitosamente";
            return RedirectToAction(nameof(Index));
        }
    }
}
