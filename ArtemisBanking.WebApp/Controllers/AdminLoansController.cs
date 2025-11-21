using ArtemisBanking.Application.Dtos.Loan;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBanking.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminLoansController : Controller
    {
        private readonly IAdminLoanService _adminLoanService;
        private readonly IMapper _mapper;

        public AdminLoansController(IAdminLoanService adminLoanService, IMapper mapper)
        {
            _adminLoanService = adminLoanService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string? statusFilter = null, string? cedulaFilter = null, CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            const int pageSize = 20;

            var result = await _adminLoanService.GetLoansAsync(page, pageSize, statusFilter, cedulaFilter, cancellationToken);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.CedulaFilter = cedulaFilter;

            var viewModels = _mapper.Map<List<LoanListItemViewModel>>(result.Items);
            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> AssignStep1(string? cedulaFilter = null, CancellationToken cancellationToken = default)
        {
            var averageDebt = await _adminLoanService.GetAverageDebtAsync(cancellationToken);
            var clients = await _adminLoanService.GetEligibleClientsAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(cedulaFilter))
            {
                clients = clients.Where(c => c.Cedula.Contains(cedulaFilter)).ToList();
            }

            ViewBag.AverageDebt = averageDebt;
            ViewBag.CedulaFilter = cedulaFilter;
            var viewModels = _mapper.Map<List<ClientForLoanViewModel>>(clients);
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

            var clients = await _adminLoanService.GetEligibleClientsAsync(cancellationToken);
            var client = clients.FirstOrDefault(c => c.UserId == userId);

            if (client == null)
            {
                TempData["ErrorMessage"] = "Cliente no encontrado o no elegible";
                return RedirectToAction(nameof(AssignStep1));
            }

            var averageDebt = await _adminLoanService.GetAverageDebtAsync(cancellationToken);
            ViewBag.Client = _mapper.Map<ClientForLoanViewModel>(client);
            ViewBag.AverageDebt = averageDebt;
            ViewBag.CurrentDebt = client.DeudaTotal;

            return View(new AssignLoanViewModel { UserId = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStep2(AssignLoanViewModel model, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var clients = await _adminLoanService.GetEligibleClientsAsync(cancellationToken);
                var client = clients.FirstOrDefault(c => c.UserId == model.UserId);
                if (client != null)
                {
                    var averageDebt = await _adminLoanService.GetAverageDebtAsync(cancellationToken);
                    ViewBag.Client = _mapper.Map<ClientForLoanViewModel>(client);
                    ViewBag.AverageDebt = averageDebt;
                    ViewBag.CurrentDebt = client.DeudaTotal;
                }
                return View(model);
            }

            if (model.PlazoMeses % 6 != 0)
            {
                ModelState.AddModelError("PlazoMeses", "El plazo debe ser un múltiplo de 6 meses");
                var clients = await _adminLoanService.GetEligibleClientsAsync(cancellationToken);
                var client = clients.FirstOrDefault(c => c.UserId == model.UserId);
                if (client != null)
                {
                    var averageDebt = await _adminLoanService.GetAverageDebtAsync(cancellationToken);
                    ViewBag.Client = _mapper.Map<ClientForLoanViewModel>(client);
                    ViewBag.AverageDebt = averageDebt;
                    ViewBag.CurrentDebt = client.DeudaTotal;
                }
                return View(model);
            }

            var clientsForRisk = await _adminLoanService.GetEligibleClientsAsync(cancellationToken);
            var clientForRisk = clientsForRisk.FirstOrDefault(c => c.UserId == model.UserId);
            var averageDebtForRisk = await _adminLoanService.GetAverageDebtAsync(cancellationToken);

            var tasaMensual = model.TasaInteres / 100 / 12;
            var montoTotalConInteres = model.MontoCapital * (decimal)Math.Pow(1 + (double)tasaMensual, model.PlazoMeses);
            var nuevaDeudaTotal = (clientForRisk?.DeudaTotal ?? 0) + montoTotalConInteres;

            bool needsWarning = false;
            string warningMessage = "";

            if (averageDebtForRisk > 0)
            {
                if (clientForRisk != null && clientForRisk.DeudaTotal > averageDebtForRisk)
                {
                    needsWarning = true;
                    warningMessage = "Este cliente se considera de alto riesgo, ya que su deuda actual supera el promedio del sistema";
                }
                else if (nuevaDeudaTotal > averageDebtForRisk)
                {
                    needsWarning = true;
                    warningMessage = "Asignar este préstamo convertirá al cliente en un cliente de alto riesgo, ya que su deuda superará el umbral promedio del sistema";
                }
            }

            if (needsWarning)
            {
                ViewBag.NeedsWarning = true;
                ViewBag.WarningMessage = warningMessage;
                ViewBag.Client = _mapper.Map<ClientForLoanViewModel>(clientForRisk);
                ViewBag.AverageDebt = averageDebtForRisk;
                ViewBag.CurrentDebt = clientForRisk?.DeudaTotal ?? 0;
                return View(model);
            }

            if (clientForRisk != null && clientForRisk.DeudaTotal > averageDebtForRisk)
            {
                needsWarning = true;
                warningMessage = "Este cliente se considera de alto riesgo, ya que su deuda actual supera el promedio del sistema";
            }
            else if (nuevaDeudaTotal > averageDebtForRisk)
            {
                needsWarning = true;
                warningMessage = "Asignar este préstamo convertirá al cliente en un cliente de alto riesgo, ya que su deuda superará el umbral promedio del sistema";
            }

            if (needsWarning)
            {
                ViewBag.NeedsWarning = true;
                ViewBag.WarningMessage = warningMessage;
                ViewBag.Client = _mapper.Map<ClientForLoanViewModel>(clientForRisk);
                ViewBag.AverageDebt = averageDebtForRisk;
                ViewBag.CurrentDebt = clientForRisk?.DeudaTotal ?? 0;
                return View(model);
            }

            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentAdminId))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al administrador";
                return RedirectToAction(nameof(Index));
            }

            var dto = _mapper.Map<AssignLoanDTO>(model);
            var result = await _adminLoanService.AssignLoanAsync(dto, currentAdminId, false, cancellationToken);

            if (result.IsFailure)
            {
                ModelState.AddModelError("", result.GeneralError ?? "Error al asignar el préstamo");
                var clients = await _adminLoanService.GetEligibleClientsAsync(cancellationToken);
                var client = clients.FirstOrDefault(c => c.UserId == model.UserId);
                if (client != null)
                {
                    var averageDebt = await _adminLoanService.GetAverageDebtAsync(cancellationToken);
                    ViewBag.Client = _mapper.Map<ClientForLoanViewModel>(client);
                    ViewBag.AverageDebt = averageDebt;
                    ViewBag.CurrentDebt = client.DeudaTotal;
                }
                return View(model);
            }

            TempData["SuccessMessage"] = "Préstamo asignado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAssign(AssignLoanViewModel model, CancellationToken cancellationToken = default)
        {
            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentAdminId))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al administrador";
                return RedirectToAction(nameof(Index));
            }

            var dto = _mapper.Map<AssignLoanDTO>(model);
            var result = await _adminLoanService.AssignLoanAsync(dto, currentAdminId, true, cancellationToken);

            if (result.IsFailure)
            {
                TempData["ErrorMessage"] = result.GeneralError ?? "Error al asignar el préstamo";
                return RedirectToAction(nameof(AssignStep2), new { userId = model.UserId });
            }

            TempData["SuccessMessage"] = "Préstamo asignado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken = default)
        {
            var loan = await _adminLoanService.GetLoanByIdAsync(id, cancellationToken);
            if (loan == null)
            {
                TempData["ErrorMessage"] = "Préstamo no encontrado";
                return RedirectToAction(nameof(Index));
            }

            return View(loan);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
        {
            var loan = await _adminLoanService.GetLoanByIdAsync(id, cancellationToken);
            if (loan == null)
            {
                TempData["ErrorMessage"] = "Préstamo no encontrado";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new UpdateLoanViewModel
            {
                Id = id,
                MontoCapital = loan.MontoCapital,
                TasaInteres = loan.TasaInteres,
                PlazoMeses = loan.PlazoMeses
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateLoanViewModel model, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = _mapper.Map<UpdateLoanDTO>(model);
            var result = await _adminLoanService.UpdateLoanAsync(dto, cancellationToken);

            if (result.IsFailure)
            {
                ModelState.AddModelError("", result.GeneralError ?? "Error al actualizar el préstamo");
                return View(model);
            }

            TempData["SuccessMessage"] = "Préstamo actualizado exitosamente";
            return RedirectToAction(nameof(Index));
        }
    }
}

