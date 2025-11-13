using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Application.Dtos.Transaction;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBanking.WebApp.Controllers
{
    [Authorize]
    public class TransaccionesController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IClienteHomeService _clienteHomeService;
        private readonly IBeneficiaryService _beneficiaryService;

        public TransaccionesController(
            ITransactionService transactionService,
            IClienteHomeService clienteHomeService,
            IBeneficiaryService beneficiaryService)
        {
            _transactionService = transactionService;
            _clienteHomeService = clienteHomeService;
            _beneficiaryService = beneficiaryService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> Express()
        {
            var userId = GetUserId();
            var home = await _clienteHomeService.GetHomeDataAsync(userId);

            var vm = new TransactionExpressViewModel
            {
                CuentasDisponibles = home.CuentasDeAhorro
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Express(TransactionExpressViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.CuentasDisponibles =
                    (await _clienteHomeService.GetHomeDataAsync(GetUserId())).CuentasDeAhorro;
                return View(vm);
            }

            var dto = new CreateTransactionExpressDTO
            {
                CuentaOrigen = vm.CuentaOrigen,
                CuentaDestino = vm.CuentaDestino,
                Monto = vm.Monto,
                UserId = GetUserId()
            };

            await _transactionService.CreateTransactionExpressAsync(GetUserId(), dto);

            TempData["SuccessMessage"] = "Transferencia express realizada correctamente.";
            return RedirectToAction("Index", "Cliente");
        }

        [HttpGet]
        public async Task<IActionResult> Beneficiario()
        {
            var userId = GetUserId();

            var home = await _clienteHomeService.GetHomeDataAsync(userId);
            var beneficiarios = await _beneficiaryService.GetBeneficiariesAsync(userId);

            var vm = new TransactionBeneficiaryViewModel
            {
                BeneficiariosDisponibles = beneficiarios,
                CuentasDisponibles = home.CuentasDeAhorro
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Beneficiario(TransactionBeneficiaryViewModel vm)
        {
            var userId = GetUserId();

            if (!ModelState.IsValid)
            {
                vm.BeneficiariosDisponibles = await _beneficiaryService.GetBeneficiariesAsync(userId);
                vm.CuentasDisponibles = (await _clienteHomeService.GetHomeDataAsync(userId)).CuentasDeAhorro;
                return View(vm);
            }

            var dto = new CreateTransactionBeneficiaryDTO
            {
                BeneficiaryId = vm.BeneficiaryId,
                CuentaOrigen = vm.CuentaOrigen,
                Monto = vm.Monto,
                UserId = userId
            };

            await _transactionService.CreateTransactionToBeneficiaryAsync(userId, dto);

            TempData["SuccessMessage"] = "Transferencia a beneficiario realizada correctamente.";
            return RedirectToAction("Index", "Cliente");
        }

        [HttpGet]
        public async Task<IActionResult> EntreCuentas()
        {
            var userId = GetUserId();
            var home = await _clienteHomeService.GetHomeDataAsync(userId);

            var vm = new TransferBetweenAccountsViewModel
            {
                CuentasDisponibles = home.CuentasDeAhorro
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EntreCuentas(TransferBetweenAccountsViewModel vm)
        {
            var userId = GetUserId();

            if (!ModelState.IsValid)
            {
                vm.CuentasDisponibles =
                    (await _clienteHomeService.GetHomeDataAsync(userId)).CuentasDeAhorro;
                return View(vm);
            }

            var dto = new TransferBetweenAccountsDTO
            {
                CuentaOrigen = vm.CuentaOrigen,
                CuentaDestino = vm.CuentaDestino,
                Monto = vm.Monto,
                UserId = userId
            };

            await _transactionService.TransferBetweenAccountsAsync(userId, dto);

            TempData["SuccessMessage"] = "Transferencia entre cuentas realizada correctamente.";
            return RedirectToAction("Index", "Cliente");
        }

        [HttpGet]
        public async Task<IActionResult> PagarPrestamo()
        {
            var userId = GetUserId();
            var home = await _clienteHomeService.GetHomeDataAsync(userId);

            var vm = new PayLoanViewModel
            {
                PrestamosDisponibles = home.Prestamos,
                CuentasDisponibles = home.CuentasDeAhorro
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarPrestamo(PayLoanViewModel vm)
        {
            var userId = GetUserId();

            if (!ModelState.IsValid)
            {
                var home = await _clienteHomeService.GetHomeDataAsync(userId);
                vm.PrestamosDisponibles = home.Prestamos;
                vm.CuentasDisponibles = home.CuentasDeAhorro;
                return View(vm);
            }

            var dto = new PayLoanDTO
            {
                LoanId = vm.LoanId,
                CuentaOrigen = vm.CuentaOrigen,
                Monto = vm.Monto,
                UserId = userId
            };

            await _transactionService.PayLoanAsync(userId, dto);

            TempData["SuccessMessage"] = "Pago de préstamo realizado correctamente.";
            return RedirectToAction("Index", "Cliente");
        }

        [HttpGet]
        public async Task<IActionResult> PagarTarjeta()
        {
            var userId = GetUserId();
            var home = await _clienteHomeService.GetHomeDataAsync(userId);

            var vm = new PayCreditCardViewModel
            {
                TarjetasDisponibles = home.TarjetasDeCredito,
                CuentasDisponibles = home.CuentasDeAhorro
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarTarjeta(PayCreditCardViewModel vm)
        {
            var userId = GetUserId();

            if (!ModelState.IsValid)
            {
                var home = await _clienteHomeService.GetHomeDataAsync(userId);
                vm.TarjetasDisponibles = home.TarjetasDeCredito;
                vm.CuentasDisponibles = home.CuentasDeAhorro;
                return View(vm);
            }

            var dto = new PayCreditCardDTO
            {
                CreditCardId = vm.CreditCardId,
                CuentaOrigen = vm.CuentaOrigen,
                Monto = vm.Monto,
                UserId = userId
            };

            await _transactionService.PayCreditCardAsync(userId, dto);

            TempData["SuccessMessage"] = "Pago de tarjeta realizado correctamente.";
            return RedirectToAction("Index", "Cliente");
        }

        [HttpGet]
        public async Task<IActionResult> AvanceEfectivo()
        {
            var userId = GetUserId();
            var home = await _clienteHomeService.GetHomeDataAsync(userId);

            var vm = new CashAdvanceViewModel
            {
                TarjetasDisponibles = home.TarjetasDeCredito,
                CuentasDisponibles = home.CuentasDeAhorro
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AvanceEfectivo(CashAdvanceViewModel vm)
        {
            var userId = GetUserId();

            if (!ModelState.IsValid)
            {
                var home = await _clienteHomeService.GetHomeDataAsync(userId);
                vm.TarjetasDisponibles = home.TarjetasDeCredito;
                vm.CuentasDisponibles = home.CuentasDeAhorro;
                return View(vm);
            }

            var dto = new CashAdvanceDTO
            {
                CreditCardId = vm.CreditCardId,
                CuentaDestino = vm.CuentaDestino,
                Monto = vm.Monto,
                UserId = userId
            };

            await _transactionService.CreateCashAdvanceAsync(userId, dto);

            TempData["SuccessMessage"] = "Avance de efectivo realizado correctamente.";
            return RedirectToAction("Index", "Cliente");
        }
    }
}
