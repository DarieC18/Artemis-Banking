using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Application.Dtos.Transaction;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels.Cliente;
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
        private readonly IUserInfoService _userInfoService;
        private readonly ISavingsAccountRepository _savingsAccountRepository;

        public TransaccionesController(
            ITransactionService transactionService,
            IClienteHomeService clienteHomeService,
            IBeneficiaryService beneficiaryService,
            IUserInfoService userInfoService,
            ISavingsAccountRepository savingsAccountRepository)
        {
            _transactionService = transactionService;
            _clienteHomeService = clienteHomeService;
            _beneficiaryService = beneficiaryService;
            _userInfoService = userInfoService;
            _savingsAccountRepository = savingsAccountRepository;
        }
        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        //Expres
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
            var userId = GetUserId();

            ModelState.Remove(nameof(vm.CuentasDisponibles));

            if (!ModelState.IsValid)
            {
                vm.CuentasDisponibles =
                    (await _clienteHomeService.GetHomeDataAsync(userId)).CuentasDeAhorro;
                return View(vm);
            }

            var cuentaDestino = await _savingsAccountRepository.GetByAccountNumberAsync(vm.CuentaDestino);

            if (cuentaDestino == null || !cuentaDestino.IsActive)
            {
                ModelState.AddModelError("", "La cuenta destino no existe o está inactiva.");

                vm.CuentasDisponibles =
                    (await _clienteHomeService.GetHomeDataAsync(userId)).CuentasDeAhorro;

                return View(vm);
            }

            var infoTitular = await _userInfoService.GetUserBasicInfoByIdAsync(cuentaDestino.UserId);
            var nombreDestino = $"{infoTitular.Nombre} {infoTitular.Apellido}".Trim();

            var confirmVm = new ConfirmTransactionExpressViewModel
            {
                CuentaOrigen = vm.CuentaOrigen,
                CuentaDestino = vm.CuentaDestino,
                Monto = vm.Monto,
                NombreTitularDestino = nombreDestino
            };

            return View("ConfirmExpress", confirmVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmExpress(ConfirmTransactionExpressViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfirmExpress", vm);
            }

            var userId = GetUserId();

            var dto = new CreateTransactionExpressDTO
            {
                CuentaOrigen = vm.CuentaOrigen,
                CuentaDestino = vm.CuentaDestino,
                Monto = vm.Monto,
                UserId = userId
            };

            try
            {
                await _transactionService.CreateTransactionExpressAsync(userId, dto);

                TempData["SuccessMessage"] = "Transferencia express realizada correctamente.";
                return RedirectToAction("Index", "Cliente");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                var home = await _clienteHomeService.GetHomeDataAsync(userId);

                var expressVm = new TransactionExpressViewModel
                {
                    CuentaOrigen = vm.CuentaOrigen,
                    CuentaDestino = vm.CuentaDestino,
                    Monto = vm.Monto,
                    CuentasDisponibles = home.CuentasDeAhorro
                };

                return View("Express", expressVm);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al procesar la transacción.";

                var home = await _clienteHomeService.GetHomeDataAsync(userId);

                var expressVm = new TransactionExpressViewModel
                {
                    CuentaOrigen = vm.CuentaOrigen,
                    CuentaDestino = vm.CuentaDestino,
                    Monto = vm.Monto,
                    CuentasDisponibles = home.CuentasDeAhorro
                };

                return View("Express", expressVm);
            }
        }

        //Beneficiario
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
            ModelState.Remove(nameof(vm.BeneficiariosDisponibles));
            ModelState.Remove(nameof(vm.CuentasDisponibles));

            if (!ModelState.IsValid)
            {
                vm.BeneficiariosDisponibles = await _beneficiaryService.GetBeneficiariesAsync(userId);
                vm.CuentasDisponibles = (await _clienteHomeService.GetHomeDataAsync(userId)).CuentasDeAhorro;
                return View(vm);
            }

            var beneficiarios = await _beneficiaryService.GetBeneficiariesAsync(userId);
            var beneficiario = beneficiarios.FirstOrDefault(b => b.Id == vm.BeneficiaryId);

            if (beneficiario == null)
            {
                ModelState.AddModelError(string.Empty, "El beneficiario seleccionado no existe.");
                vm.BeneficiariosDisponibles = beneficiarios;
                vm.CuentasDisponibles = (await _clienteHomeService.GetHomeDataAsync(userId)).CuentasDeAhorro;
                return View(vm);
            }

            var confirmVm = new ConfirmTransactionBeneficiaryViewModel
            {
                BeneficiaryId = beneficiario.Id,
                BeneficiaryNombre = beneficiario.NombreCompleto,
                NumeroCuentaBeneficiario = beneficiario.NumeroCuentaBeneficiario,
                CuentaOrigen = vm.CuentaOrigen,
                Monto = vm.Monto
            };

            return View("ConfirmBeneficiario", confirmVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBeneficiario(ConfirmTransactionBeneficiaryViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfirmBeneficiario", vm);
            }

            var userId = GetUserId();

            var dto = new CreateTransactionBeneficiaryDTO
            {
                BeneficiaryId = vm.BeneficiaryId,
                CuentaOrigen = vm.CuentaOrigen,
                Monto = vm.Monto,
                UserId = userId
            };

            try
            {
                await _transactionService.CreateTransactionToBeneficiaryAsync(userId, dto);

                TempData["SuccessMessage"] = "Transferencia a beneficiario realizada correctamente.";
                return RedirectToAction("Index", "Cliente");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return View("ConfirmBeneficiario", vm);
            }
            catch
            {
                TempData["ErrorMessage"] = "Ocurrió un error al procesar la transferencia a beneficiario.";
                return View("ConfirmBeneficiario", vm);
            }
        }

        //Entre cuentras
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

            ModelState.Remove(nameof(vm.CuentasDisponibles));

            if (!ModelState.IsValid)
            {
                var home = await _clienteHomeService.GetHomeDataAsync(userId);
                vm.CuentasDisponibles = home.CuentasDeAhorro;
                return View(vm);
            }

            var dto = new TransferBetweenAccountsDTO
            {
                CuentaOrigen = vm.CuentaOrigen,
                CuentaDestino = vm.CuentaDestino,
                Monto = vm.Monto,
                UserId = userId
            };

            try
            {
                await _transactionService.TransferBetweenAccountsAsync(userId, dto);

                TempData["SuccessMessage"] = "Transferencia entre cuentas realizada correctamente.";
                return RedirectToAction("Index", "Cliente");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                var home = await _clienteHomeService.GetHomeDataAsync(userId);
                vm.CuentasDisponibles = home.CuentasDeAhorro;

                return View(vm);
            }
            catch
            {
                TempData["ErrorMessage"] = "Ocurrió un error al procesar la transferencia.";

                var home = await _clienteHomeService.GetHomeDataAsync(userId);
                vm.CuentasDisponibles = home.CuentasDeAhorro;

                return View(vm);
            }
        }


        //Prestamo
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

            ModelState.Remove(nameof(vm.PrestamosDisponibles));
            ModelState.Remove(nameof(vm.CuentasDisponibles));

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

            try
            {
                await _transactionService.PayLoanAsync(userId, dto);

                TempData["SuccessMessage"] = "Pago de préstamo realizado correctamente.";
                return RedirectToAction("Index", "Cliente");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                var home = await _clienteHomeService.GetHomeDataAsync(userId);
                vm.PrestamosDisponibles = home.Prestamos;
                vm.CuentasDisponibles = home.CuentasDeAhorro;

                return View(vm);
            }
            catch
            {
                TempData["ErrorMessage"] = "Ocurrió un error al procesar el pago del préstamo.";

                var home = await _clienteHomeService.GetHomeDataAsync(userId);
                vm.PrestamosDisponibles = home.Prestamos;
                vm.CuentasDisponibles = home.CuentasDeAhorro;

                return View(vm);
            }
        }

        //TC
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

            ModelState.Remove(nameof(vm.TarjetasDisponibles));
            ModelState.Remove(nameof(vm.CuentasDisponibles));

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

            try
            {
                await _transactionService.PayCreditCardAsync(userId, dto);

                TempData["SuccessMessage"] = "Pago de tarjeta realizado correctamente.";
                return RedirectToAction("Index", "Cliente");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                var home = await _clienteHomeService.GetHomeDataAsync(userId);
                vm.TarjetasDisponibles = home.TarjetasDeCredito;
                vm.CuentasDisponibles = home.CuentasDeAhorro;

                return View(vm);
            }
            catch
            {
                TempData["ErrorMessage"] = "Ocurrió un error al procesar el pago de la tarjeta.";

                var home = await _clienteHomeService.GetHomeDataAsync(userId);
                vm.TarjetasDisponibles = home.TarjetasDeCredito;
                vm.CuentasDisponibles = home.CuentasDeAhorro;

                return View(vm);
            }
        }

        //Avance efectiv
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

            ModelState.Remove(nameof(vm.TarjetasDisponibles));
            ModelState.Remove(nameof(vm.CuentasDisponibles));

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

            try
            {
                await _transactionService.CreateCashAdvanceAsync(userId, dto);

                TempData["SuccessMessage"] = "Avance de efectivo realizado correctamente.";
                return RedirectToAction("Index", "Cliente");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                var home = await _clienteHomeService.GetHomeDataAsync(userId);
                vm.TarjetasDisponibles = home.TarjetasDeCredito;
                vm.CuentasDisponibles = home.CuentasDeAhorro;

                return View(vm);
            }
            catch
            {
                // Errores 
                TempData["ErrorMessage"] = "Ocurrió un error al procesar el avance de efectivo.";

                var home = await _clienteHomeService.GetHomeDataAsync(userId);
                vm.TarjetasDisponibles = home.TarjetasDeCredito;
                vm.CuentasDisponibles = home.CuentasDeAhorro;

                return View(vm);
            }
        }
    }
}
