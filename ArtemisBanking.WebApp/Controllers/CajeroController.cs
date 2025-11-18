using ArtemisBanking.Application.Dtos.Transaction;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels.Cajero;
using ArtemisBanking.Domain.Enums;
using ArtemisBanking.Infraestructure.Identity.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


[Authorize(Roles = "Cajero")]
public class CajeroController : Controller
{
    private readonly IAccountCashOperationsService _accountCashOperationsService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILoanPaymentService _loanPaymentService;
    private readonly ICreditCardPaymentService _creditCardPaymentService;
    private readonly IMapper _mapper;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICashierThirdPartyTransferService _cashierThirdPartyTransferService;

    public CajeroController(
        IAccountCashOperationsService accountCashOperationsService,
        UserManager<AppUser> userManager,
        ILoanPaymentService loanPaymentService,
        ICreditCardPaymentService creditCardPaymentService,
        IMapper mapper,
        ITransactionRepository transactionRepository,
        ICashierThirdPartyTransferService cashierThirdPartyTransferService)
    {
        _accountCashOperationsService = accountCashOperationsService;
        _userManager = userManager;
        _loanPaymentService = loanPaymentService;
        _creditCardPaymentService = creditCardPaymentService;
        _mapper = mapper;
        _transactionRepository = transactionRepository;
        _cashierThirdPartyTransferService = cashierThirdPartyTransferService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }
        var hoy = DateTime.Today;
        var mañana = hoy.AddDays(1);

        var transaccionesHoy =
            await _transactionRepository.GetByOperatorAndDateRangeAsync(
                userId, hoy, mañana);

        var totalTransacciones = transaccionesHoy.Count();

        var totalPagos = transaccionesHoy.Count(t =>
            t.OperationType == TransactionOperationType.PagoPrestamo ||
            t.OperationType == TransactionOperationType.PagoTarjetaCredito);

        var totalDepositos = transaccionesHoy.Count(t =>
            t.OperationType == TransactionOperationType.Deposito);

        var totalRetiros = transaccionesHoy.Count(t =>
            t.OperationType == TransactionOperationType.Retiro);

        var model = new CajeroHomeViewModel
        {
            TotalTransaccionesHoy = totalTransacciones,
            TotalPagosHoy = totalPagos,
            TotalDepositosHoy = totalDepositos,
            TotalRetirosHoy = totalRetiros
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Home()
    {
        return RedirectToAction(nameof(Index));
    }


    //Depsito
    [HttpGet]
    public IActionResult Deposito()
    {
        var vm = new DepositoFormViewModel();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposito(DepositoFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = _userManager.GetUserId(User);

        var dto = new DepositDTO
        {
            AccountNumber = model.AccountNumber,
            Amount = model.Amount,
            OperatedByUserId = userId!
        };

        var previewResult = await _accountCashOperationsService.PreviewDepositAsync(dto);

        if (previewResult.IsFailure)
        {
            ModelState.AddModelError(string.Empty, previewResult.GeneralError ?? "No se pudo procesar el depósito.");
            return View(model);
        }

        var preview = previewResult.Value!;

        var confirmVm = new DepositoConfirmViewModel
        {
            AccountNumber = preview.AccountNumber,
            AccountMasked = preview.AccountMasked,
            HolderFullName = preview.HolderFullName,
            Amount = preview.Amount,
            CurrentBalance = preview.CurrentBalance
        };

        return View("ConfirmarDeposito", confirmVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarDeposito(DepositoConfirmViewModel model)
    {
        var userId = _userManager.GetUserId(User);

        var command = new DepositDTO
        {
            AccountNumber = model.AccountNumber,
            Amount = model.Amount,
            OperatedByUserId = userId!
        };

        var executeResult = await _accountCashOperationsService.ExecuteDepositAsync(command);

        if (executeResult.IsFailure)
        {
            ModelState.AddModelError(string.Empty, executeResult.GeneralError ?? "No se pudo completar el depósito.");
            return View(model);
        }

        TempData["SuccessMessage"] =
            $"Depósito realizado correctamente a la cuenta {MaskForView(executeResult.Value!.AccountNumber)}.";

        return RedirectToAction(nameof(Index));
    }

    private static string MaskForView(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length < 4)
            return accountNumber;

        var last4 = accountNumber[^4..];
        return $"****{last4}";
    }

    //Retiro
    [HttpGet]
    public IActionResult Retiro()
    {
        var vm = new RetiroFormViewModel();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Retiro(RetiroFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = _userManager.GetUserId(User);

        var dto = new WithdrawalDTO
        {
            AccountNumber = model.AccountNumber,
            Amount = model.Amount,
            OperatedByUserId = userId!
        };

        var previewResult = await _accountCashOperationsService.PreviewWithdrawalAsync(dto);

        if (previewResult.IsFailure)
        {
            ModelState.AddModelError(string.Empty, previewResult.GeneralError ?? "No se pudo procesar el retiro.");
            return View(model);
        }

        var preview = previewResult.Value!;

        var confirmVm = new RetiroConfirmViewModel
        {
            AccountNumber = preview.AccountNumber,
            AccountMasked = preview.AccountMasked,
            HolderFullName = preview.HolderFullName,
            Amount = preview.Amount,
            CurrentBalance = preview.CurrentBalance
        };

        return View("RetiroConfirmacion", confirmVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarRetiro(RetiroConfirmViewModel model)
    {
        var userId = _userManager.GetUserId(User);

        var command = new WithdrawalDTO
        {
            AccountNumber = model.AccountNumber,
            Amount = model.Amount,
            OperatedByUserId = userId!
        };

        var executeResult = await _accountCashOperationsService.ExecuteWithdrawalAsync(command);

        if (executeResult.IsFailure)
        {
            ModelState.AddModelError(string.Empty, executeResult.GeneralError ?? "No se pudo completar el retiro.");
            return View("RetiroConfirmacion", model);
        }

        TempData["SuccessMessage"] =
            $"Retiro realizado correctamente de la cuenta {MaskForView(executeResult.Value!.AccountNumber)}.";

        return RedirectToAction(nameof(Index));
    }
    //Pago Tarjetas
    [HttpGet]
    public IActionResult PagoTarjeta()
    {
        var vm = new PagoTarjetaFormViewModel();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PagoTarjeta(PagoTarjetaFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = _userManager.GetUserId(User);

        var dto = _mapper.Map<PayCreditCardDTO>(model);
        dto.UserId = userId!;

        var previewResult = await _creditCardPaymentService.PreviewPayCreditCardAsync(dto);

        if (previewResult.IsFailure)
        {
            ModelState.AddModelError(string.Empty,
                previewResult.GeneralError ?? "No se pudo procesar el pago de la tarjeta.");
            return View(model);
        }

        var preview = previewResult.Value!;

        var confirmVm = _mapper.Map<PagoTarjetaConfirmViewModel>(preview);
        confirmVm.CreditCardId = model.CreditCardId;
        confirmVm.MontoSolicitado = model.Monto;

        return View("ConfirmarPagoTarjeta", confirmVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarPagoTarjeta(PagoTarjetaConfirmViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = _userManager.GetUserId(User);

        var command = new PayCreditCardDTO
        {
            CreditCardId = model.CreditCardId,
            CuentaOrigen = model.CuentaOrigen,
            Monto = model.MontoSolicitado,
            UserId = userId!
        };

        var executeResult = await _creditCardPaymentService.ExecutePayCreditCardAsync(command);

        if (executeResult.IsFailure)
        {
            ModelState.AddModelError(string.Empty,
                executeResult.GeneralError ?? "No se pudo completar el pago de la tarjeta.");
            model.ErrorMessage = executeResult.GeneralError;
            return View("ConfirmarPagoTarjeta", model);
        }

        var result = executeResult.Value!;

        TempData["SuccessMessage"] =
            $"Pago a tarjeta {MaskForView(result.CardNumberMasked)} realizado correctamente " +
            $"desde la cuenta {MaskForView(result.SourceAccountNumber)}.";

        return RedirectToAction(nameof(Index));
    }

    //Pago Prestamos
    [HttpGet]
    public IActionResult PagoPrestamo()
    {
        var vm = new PagoPrestamoFormViewModel();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PagoPrestamo(PagoPrestamoFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = _userManager.GetUserId(User);

        var dto = new PayLoanDTO
        {
            LoanId = model.LoanId,
            CuentaOrigen = model.CuentaOrigen,
            Monto = model.Monto,
            UserId = userId!
        };

        var previewResult = await _loanPaymentService.PreviewPayLoanAsync(dto);

        if (previewResult.IsFailure || previewResult.Value is null)
        {
            var error = previewResult.GeneralError ?? "No se pudo generar la vista previa del pago de préstamo.";
            ModelState.AddModelError(string.Empty, error);
            model.ErrorMessage = error;
            return View(model);
        }

        var preview = previewResult.Value;

        var confirmVm = new PagoPrestamoConfirmViewModel
        {
            CuentaOrigen = preview.SourceAccountNumber,
            CuentaOrigenEnmascarada = preview.SourceAccountMasked,
            BalanceActualCuentaOrigen = preview.SourceCurrentBalance,

            LoanId = model.LoanId,
            NumeroPrestamo = preview.LoanNumber,
            TitularPrestamo = preview.LoanHolderFullName,
            DeudaPendienteActual = preview.TotalDebtRemaining,

            MontoSolicitado = preview.RequestedAmount,
            CuotasAfectadas = preview.InstallmentsToAffect
        };

        return View("PagoPrestamoConfirmacion", confirmVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarPagoPrestamo(PagoPrestamoConfirmViewModel model)
    {
        if (!ModelState.IsValid)
            return View("PagoPrestamoConfirmacion", model);

        var userId = _userManager.GetUserId(User);

        var command = new PayLoanDTO
        {
            LoanId = model.LoanId,
            CuentaOrigen = model.CuentaOrigen,
            Monto = model.MontoSolicitado,
            UserId = userId!
        };

        var executeResult = await _loanPaymentService.ExecutePayLoanAsync(command);

        if (executeResult.IsFailure || executeResult.Value is null)
        {
            var error = executeResult.GeneralError ?? "No se pudo completar el pago de préstamo.";
            ModelState.AddModelError(string.Empty, error);
            model.ErrorMessage = error;
            return View("PagoPrestamoConfirmacion", model);
        }

        var result = executeResult.Value;

        TempData["SuccessMessage"] =
            $"Pago realizado al préstamo {MaskForView(result.LoanNumber)} por {result.PaidAmount:C}. " +
            $"Nueva deuda pendiente: {result.NewTotalDebtRemaining:C}.";

        return RedirectToAction(nameof(Index));
    }

    //Transacciones Terceroos
    [HttpGet]
    public IActionResult TransferenciaTerceros()
    {
        var vm = new CashierThirdPartyTransferViewModel();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TransferenciaTerceros(CashierThirdPartyTransferViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = _userManager.GetUserId(User);

        var dto = new CashierThirdPartyTransferDTO
        {
            SourceAccountNumber = model.CuentaOrigen,
            DestinationAccountNumber = model.CuentaDestino,
            Amount = model.Monto,
            OperatedByUserId = userId!
        };

        var previewResult = await _cashierThirdPartyTransferService.PreviewAsync(dto);

        if (previewResult.IsFailure)
        {
            ModelState.AddModelError(string.Empty,
                previewResult.GeneralError ?? "No se pudo procesar la transferencia.");
            return View(model);
        }

        var preview = previewResult.Value!;

        var confirmVm = new CashierThirdPartyTransferConfirmViewModel
        {
            CuentaOrigen = preview.SourceAccountNumber,
            CuentaDestino = preview.DestinationAccountNumber,
            TitularDestino = preview.DestinationHolderFullName,
            Monto = preview.Amount
        };

        return View("TransferenciaTercerosConfirmacion", confirmVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarTransferenciaTerceros(CashierThirdPartyTransferConfirmViewModel model)
    {
        if (!ModelState.IsValid)
            return View("TransferenciaTercerosConfirmacion", model);

        var userId = _userManager.GetUserId(User);

        var command = new CashierThirdPartyTransferDTO
        {
            SourceAccountNumber = model.CuentaOrigen,
            DestinationAccountNumber = model.CuentaDestino,
            Amount = model.Monto,
            OperatedByUserId = userId!
        };

        var executeResult = await _cashierThirdPartyTransferService.ExecuteAsync(command);

        if (executeResult.IsFailure)
        {
            ModelState.AddModelError(string.Empty,
                executeResult.GeneralError ?? "No se pudo completar la transferencia.");
            return View("TransferenciaTercerosConfirmacion", model);
        }

        var result = executeResult.Value!;

        TempData["SuccessMessage"] =
            $"Transferencia realizada correctamente desde la cuenta {MaskForView(result.SourceAccountNumber)} " +
            $"hacia la cuenta {MaskForView(result.DestinationAccountNumber)} por {result.Amount:C}.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CancelarTransferenciaTerceros()
    {
        return RedirectToAction(nameof(Index));
    }
}
