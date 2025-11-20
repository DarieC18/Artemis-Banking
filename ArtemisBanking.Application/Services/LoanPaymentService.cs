using ArtemisBanking.Application.Dtos.Loan;
using ArtemisBanking.Application.Dtos.Transaction;
using ArtemisBanking.Application.DTOs.Email;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Enums;

namespace ArtemisBanking.Application.Services
{
    public class LoanPaymentService : ILoanPaymentService
    {
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanPaymentScheduleRepository _loanPaymentScheduleRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly IEmailService _emailService;

        public LoanPaymentService(
            ISavingsAccountRepository savingsAccountRepository,
            ILoanRepository loanRepository,
            ILoanPaymentScheduleRepository loanPaymentScheduleRepository,
            ITransactionRepository transactionRepository,
            IUserInfoService userInfoService,
            IEmailService emailService)
        {
            _savingsAccountRepository = savingsAccountRepository;
            _loanRepository = loanRepository;
            _loanPaymentScheduleRepository = loanPaymentScheduleRepository;
            _transactionRepository = transactionRepository;
            _userInfoService = userInfoService;
            _emailService = emailService;
        }

        public async Task<Result<PayLoanPreviewDTO>> PreviewPayLoanAsync(PayLoanDTO request)
        {
            if (request.Monto <= 0)
                return Result<PayLoanPreviewDTO>.Fail("El monto a pagar debe ser mayor que cero.");

            var account = await _savingsAccountRepository
                .GetByAccountNumberAsync(request.CuentaOrigen);

            if (account is null)
                return Result<PayLoanPreviewDTO>.Fail("La cuenta origen no existe.");

            if (!account.IsActive)
                return Result<PayLoanPreviewDTO>.Fail("La cuenta origen está inactiva.");

            if (account.Balance < request.Monto)
                return Result<PayLoanPreviewDTO>.Fail("Fondos insuficientes en la cuenta de origen.");

            var loan = await _loanRepository.GetByNumberWithScheduleAsync(request.LoanNumber);

            if (loan is null)
                return Result<PayLoanPreviewDTO>.Fail("El préstamo no existe.");
            request.LoanId = loan.Id;


            if (!loan.IsActive)
                return Result<PayLoanPreviewDTO>.Fail("El préstamo no está activo.");

            if (loan.MontoPendiente <= 0)
                return Result<PayLoanPreviewDTO>.Fail("El préstamo no tiene deuda pendiente.");

            var (cuotasAfectadas, montoAplicado, cambio) =
                CalcularAplicacionPago(loan.TablaAmortizacion, request.Monto);

            if (!cuotasAfectadas.Any())
                return Result<PayLoanPreviewDTO>.Fail("El monto indicado no alcanza para cubrir ninguna cuota completa.");

            var loanUser = await _userInfoService.GetUserBasicInfoByIdAsync(loan.UserId);
            var holderFullName = loanUser is null
                ? "Cliente"
                : $"{loanUser.Nombre} {loanUser.Apellido}".Trim();

            var preview = new PayLoanPreviewDTO
            {
                InternalLoanId = loan.Id,
                SourceAccountNumber = account.NumeroCuenta,
                SourceAccountMasked = MaskAccountNumber(account.NumeroCuenta),
                SourceCurrentBalance = account.Balance,

                LoanNumber = loan.NumeroPrestamo,
                LoanHolderFullName = holderFullName,
                TotalDebtRemaining = loan.MontoPendiente,

                RequestedAmount = request.Monto,
                InstallmentsToAffect = cuotasAfectadas
                    .OrderBy(c => c.NumeroCuota)
                    .Select(c => new LoanPaymentScheduleDTO
                    {
                        NumeroCuota = c.NumeroCuota,
                        ValorCuota = c.ValorCuota,
                        FechaPago = c.FechaPago,
                        Pagada = c.Pagada,
                        Atrasada = c.Atrasada
                    })
                    .ToList()
            };

            return Result<PayLoanPreviewDTO>.Ok(preview);
        }

        public async Task<Result<PayLoanResultDTO>> ExecutePayLoanAsync(PayLoanDTO command)
        {
            if (command.Monto <= 0)
                return Result<PayLoanResultDTO>.Fail("El monto a pagar debe ser mayor que cero.");

            var now = DateTime.Now;

            var account = await _savingsAccountRepository
                .GetByAccountNumberAsync(command.CuentaOrigen);

            if (account is null)
                return Result<PayLoanResultDTO>.Fail("La cuenta origen no existe.");

            if (!account.IsActive)
                return Result<PayLoanResultDTO>.Fail("La cuenta origen está inactiva.");

            if (account.Balance < command.Monto)
                return Result<PayLoanResultDTO>.Fail("Fondos insuficientes en la cuenta de origen.");

            var loan = await _loanRepository.GetByIdWithScheduleAsync(command.LoanId);

            if (loan is null)
                return Result<PayLoanResultDTO>.Fail("El préstamo no existe.");

            if (!loan.IsActive)
                return Result<PayLoanResultDTO>.Fail("El préstamo no está activo.");

            if (loan.MontoPendiente <= 0)
                return Result<PayLoanResultDTO>.Fail("El préstamo no tiene deuda pendiente.");

            var cuotasPendientes = loan.TablaAmortizacion
                .Where(c => !c.Pagada)
                .OrderBy(c => c.NumeroCuota)
                .ToList();

            if (!cuotasPendientes.Any())
                return Result<PayLoanResultDTO>.Fail("El préstamo no tiene cuotas pendientes por pagar.");

            decimal montoDisponible = command.Monto;
            decimal montoAplicado = 0m;
            var cuotasAfectadas = new List<LoanPaymentSchedule>();

            foreach (var cuota in cuotasPendientes)
            {
                if (montoDisponible <= 0)
                    break;

                var saldoCuota = cuota.SaldoPendiente > 0 ? cuota.SaldoPendiente : cuota.ValorCuota;

                if (saldoCuota <= 0)
                    continue;

                if (montoDisponible >= saldoCuota)
                {
                    montoAplicado += saldoCuota;
                    montoDisponible -= saldoCuota;

                    cuota.SaldoPendiente = 0m;
                    cuota.Pagada = true;
                    cuota.Atrasada = false;

                    cuotasAfectadas.Add(cuota);
                }
                else
                {
                    montoAplicado += montoDisponible;

                    var nuevoSaldo = saldoCuota - montoDisponible;
                    cuota.SaldoPendiente = nuevoSaldo;

                    cuotasAfectadas.Add(cuota);
                    montoDisponible = 0m;
                    break;
                }
            }

            if (montoAplicado <= 0 || !cuotasAfectadas.Any())
                return Result<PayLoanResultDTO>.Fail("El monto indicado no aplica a ninguna cuota pendiente.");

            var cambio = command.Monto - montoAplicado;

            loan.MontoPendiente = Math.Max(0, loan.MontoPendiente - montoAplicado);
            loan.CuotasPagadas = loan.TablaAmortizacion.Count(c => c.Pagada);

            var hayCuotasAtrasadasSinPagar = loan.TablaAmortizacion.Any(c => c.Atrasada && !c.Pagada);
            loan.EstadoPago = hayCuotasAtrasadasSinPagar ? "En mora" : "Al dia";

            account.Balance -= command.Monto;

            if (cambio > 0)
            {
                account.Balance += cambio;
            }

            var transaction = new Transaction
            {
                SavingsAccountId = account.Id,
                Monto = montoAplicado,
                FechaTransaccion = now,
                Tipo = "CRÉDITO",
                Origen = account.NumeroCuenta,
                Beneficiario = loan.NumeroPrestamo,
                Estado = "APROBADA",
                OperationType = TransactionOperationType.PagoPrestamo,
                OperatedByUserId = command.UserId
            };

            await _transactionRepository.AddAsync(transaction);

            await _loanRepository.UpdateAsync(loan);
            await _loanPaymentScheduleRepository.UpdateRangeAsync(cuotasAfectadas.ToList());
            await _savingsAccountRepository.UpdateAsync(account);

            var loanUser = await _userInfoService.GetUserBasicInfoByIdAsync(loan.UserId);
            var email = loanUser?.Email ?? string.Empty;

            var result = new PayLoanResultDTO
            {
                LoanNumber = loan.NumeroPrestamo,
                PaidAmount = montoAplicado,
                NewTotalDebtRemaining = loan.MontoPendiente,
                NewSourceAccountBalance = account.Balance,
                HasChangeReturnedToSource = cambio > 0,
                ChangeAmount = cambio,
                ExecutedAt = now,
                LoanHolderEmail = email
            };

            if (!string.IsNullOrWhiteSpace(email))
            {
                var subject = $"Pago realizado al préstamo {loan.NumeroPrestamo}";
                var body = $"""
                    Hola,

                    Se ha registrado un pago a su préstamo.

                    Monto pagado: {montoAplicado:C}
                    Préstamo: {loan.NumeroPrestamo}
                    Cuenta de origen: {MaskAccountNumber(account.NumeroCuenta)}
                    Fecha y hora: {now:dd/MM/yyyy HH:mm}

                    Deuda pendiente luego de este pago: {loan.MontoPendiente:C}

                    Si usted no reconoce esta operación, contacte al banco de inmediato.

                    ArtemisBanking
                    """;

                var emailRequest = new EmailRequestDto
                {
                    To = email,
                    Subject = subject,
                    Body = body
                };

                await _emailService.SendAsync(emailRequest);
            }

            return Result<PayLoanResultDTO>.Ok(result);
        }


        private static (List<LoanPaymentSchedule> cuotasAfectadas, decimal montoAplicado, decimal cambio)
            CalcularAplicacionPago(ICollection<LoanPaymentSchedule> tablaAmortizacion, decimal montoDisponible)
        {
            var cuotasPendientes = tablaAmortizacion
                .Where(c => !c.Pagada)
                .OrderBy(c => c.NumeroCuota)
                .ToList();

            var cuotasAfectadas = new List<LoanPaymentSchedule>();
            decimal montoAplicado = 0m;
            decimal restante = montoDisponible;

            foreach (var cuota in cuotasPendientes)
            {
                if (restante <= 0)
                    break;

                if (restante >= cuota.ValorCuota)
                {
                    cuotasAfectadas.Add(cuota);
                    montoAplicado += cuota.ValorCuota;
                    restante -= cuota.ValorCuota;
                }
                else
                {
                    break;
                }
            }

            var cambio = montoDisponible - montoAplicado;
            return (cuotasAfectadas, montoAplicado, cambio);
        }

        private static string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length < 4)
                return accountNumber;

            var last4 = accountNumber[^4..];
            return $"****{last4}";
        }
    }
}
