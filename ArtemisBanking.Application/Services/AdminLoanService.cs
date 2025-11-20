using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Dtos.Loan;
using ArtemisBanking.Application.DTOs.Email;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Interfaces.Identity;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Services
{
    public class AdminLoanService : IAdminLoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanPaymentScheduleRepository _scheduleRepository;
        private readonly IIdentityUserManager _identityUserManager;
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IEmailService _emailService;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly IMapper _mapper;

        public AdminLoanService(
            ILoanRepository loanRepository,
            ILoanPaymentScheduleRepository scheduleRepository,
            IIdentityUserManager identityUserManager,
            ISavingsAccountRepository savingsAccountRepository,
            ITransactionRepository transactionRepository,
            IEmailService emailService,
            ICreditCardRepository creditCardRepository,
            IMapper mapper)
        {
            _loanRepository = loanRepository;
            _scheduleRepository = scheduleRepository;
            _identityUserManager = identityUserManager;
            _savingsAccountRepository = savingsAccountRepository;
            _transactionRepository = transactionRepository;
            _emailService = emailService;
            _creditCardRepository = creditCardRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<LoanListItemDTO>> GetLoansAsync(
            int pageNumber,
            int pageSize,
            string? statusFilter = null,
            string? cedulaFilter = null,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 20;

            List<Loan> loans;

            if (!string.IsNullOrWhiteSpace(cedulaFilter))
            {
                var allUsers = await _identityUserManager.GetAllAsync(cancellationToken);
                var user = allUsers.FirstOrDefault(u => u.Cedula == cedulaFilter);
                if (user == null)
                {
                    return new PaginatedResult<LoanListItemDTO>(new List<LoanListItemDTO>(), pageNumber, pageSize, 0);
                }

                loans = await _loanRepository.GetByUserIdAsync(user.Id);
                loans = loans.OrderByDescending(l => l.FechaCreacion).ToList();
            }
            else
            {
                if (statusFilter == "Completados")
                {
                    loans = await _loanRepository.GetAllCompletedAsync();
                }
                else
                {
                    loans = await _loanRepository.GetAllActiveAsync();
                }
            }

            var totalCount = loans.Count;
            var pagedLoans = loans
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtoList = new List<LoanListItemDTO>();
            foreach (var loan in pagedLoans)
            {
                var user = await _identityUserManager.GetByIdAsync(loan.UserId, cancellationToken);
                var estadoPago = CalculatePaymentStatus(loan);

                dtoList.Add(new LoanListItemDTO
                {
                    Id = loan.Id,
                    NumeroPrestamo = loan.NumeroPrestamo,
                    ClienteNombre = user?.Nombre ?? "",
                    ClienteApellido = user?.Apellido ?? "",
                    MontoCapital = loan.MontoCapital,
                    CuotasTotales = loan.CuotasTotales,
                    CuotasPagadas = loan.CuotasPagadas,
                    MontoPendiente = loan.MontoPendiente,
                    TasaInteres = loan.TasaInteres,
                    PlazoMeses = loan.PlazoMeses,
                    EstadoPago = estadoPago,
                    IsActive = loan.IsActive
                });
            }

            return new PaginatedResult<LoanListItemDTO>(dtoList, pageNumber, pageSize, totalCount);
        }

        public async Task<LoanDetailDTO?> GetLoanByIdAsync(int loanId, CancellationToken cancellationToken = default)
        {
            var loan = await _loanRepository.GetByIdWithScheduleAsync(loanId);
            if (loan == null)
                return null;

            var estadoPago = CalculatePaymentStatus(loan);
            var schedule = _mapper.Map<List<LoanPaymentScheduleDTO>>(loan.TablaAmortizacion.OrderBy(s => s.NumeroCuota));

            return new LoanDetailDTO
            {
                NumeroPrestamo = loan.NumeroPrestamo,
                MontoCapital = loan.MontoCapital,
                CuotasTotales = loan.CuotasTotales,
                CuotasPagadas = loan.CuotasPagadas,
                MontoPendiente = loan.MontoPendiente,
                TasaInteres = loan.TasaInteres,
                PlazoMeses = loan.PlazoMeses,
                FechaCreacion = loan.FechaCreacion,
                EstadoPago = estadoPago,
                TablaAmortizacion = schedule
            };
        }

        public async Task<List<ClientForLoanDTO>> GetEligibleClientsAsync(CancellationToken cancellationToken = default)
        {
            var allUsers = await _identityUserManager.GetAllAsync(cancellationToken);
            var clientes = allUsers
                .Where(u => u.Roles.Contains("Cliente") && u.IsActive)
                .ToList();

            var eligibleClients = new List<ClientForLoanDTO>();

            foreach (var cliente in clientes)
            {
                var hasActiveLoan = await _loanRepository.HasActiveLoanAsync(cliente.Id);
                if (hasActiveLoan)
                    continue;

                var loans = await _loanRepository.GetByUserIdAsync(cliente.Id);
                var deudaTotal = loans
                    .Where(l => l.IsActive)
                    .Sum(l => l.MontoPendiente);

                eligibleClients.Add(new ClientForLoanDTO
                {
                    UserId = cliente.Id,
                    Cedula = cliente.Cedula,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Email = cliente.Email,
                    DeudaTotal = deudaTotal
                });
            }

            return eligibleClients.OrderBy(c => c.Nombre).ToList();
        }

        public async Task<decimal> GetAverageDebtAsync(CancellationToken cancellationToken = default)
        {
            return await _loanRepository.GetAverageDebtAsync();
        }

        private decimal CalcularCuotaMensualSistemaFrances(decimal montoCapital, decimal tasaAnual, int plazoMeses)
        {
            // Convertir tasa anual a mensual en decimal
            double r = (double)(tasaAnual / 100 / 12);

            double factorCrecimiento = Math.Pow(1 + r, plazoMeses);

            // Aplica la formyla
            double numerador = r * factorCrecimiento;
            double denominador = factorCrecimiento - 1;

            decimal cuota = montoCapital * (decimal)(numerador / denominador);

            return Math.Round(cuota, 2); // Redondea a centavos
        }

        // Calcula el monto total a pagar (capital + intereses) usando Sistema Francés
        private decimal CalcularMontoTotalConIntereses(decimal montoCapital, decimal tasaAnual, int plazoMeses)
        {
            var cuotaMensual = CalcularCuotaMensualSistemaFrances(montoCapital, tasaAnual, plazoMeses);
            return cuotaMensual * plazoMeses;
        }

        public async Task<Result> AssignLoanAsync(AssignLoanDTO request, string adminUserId, CancellationToken cancellationToken = default)
        {
            var user = await _identityUserManager.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
                return Result.Fail("Cliente no encontrado");

            if (await _loanRepository.HasActiveLoanAsync(request.UserId))
                return Result.Fail("El cliente ya tiene un préstamo activo");

            if (request.PlazoMeses % 6 != 0 || request.PlazoMeses < 6 || request.PlazoMeses > 60)
                return Result.Fail("El plazo debe ser un múltiplo de 6 meses entre 6 y 60 meses");

            // Valida que el cliente es o se convierte en cliente de alto riesgo
            var averageDebt = await GetAverageDebtAsync(cancellationToken);
            var existingLoans = await _loanRepository.GetByUserIdAsync(request.UserId);
            var deudaPrestamos = existingLoans.Where(l => l.IsActive).Sum(l => l.MontoPendiente);
            
            // Incluir tambien la deuda de tarjetas de crédito
            var existingCreditCards = await _creditCardRepository.GetActiveByUserIdAsync(request.UserId);
            var deudaTarjetas = existingCreditCards.Sum(c => c.DeudaActual);
            var existingDebt = deudaPrestamos + deudaTarjetas;
            
            // Calcula la deuda total que tendria con este nuevo préstamo
            var montoTotalConInteres = CalcularMontoTotalConIntereses(request.MontoCapital, request.TasaInteres, request.PlazoMeses);
            var nuevaDeudaTotal = existingDebt + montoTotalConInteres;

            // Verifica si es cliente de alto riesgo
            if (existingDebt > averageDebt)
            {
                return Result.Fail("Este cliente se considera de alto riesgo, ya que su deuda actual supera el promedio del sistema");
            }

            // Verifica si se convertiria en cliente de alto riesgo
            if (nuevaDeudaTotal > averageDebt)
            {
                return Result.Fail("Asignar este préstamo convertirá al cliente en un cliente de alto riesgo, ya que su deuda superará el umbral promedio del sistema");
            }

            //Genera el numero unico de prestamo
            var numeroPrestamo = await _loanRepository.GenerateUniqueLoanNumberAsync();

            //Calcula la cuota mensual con Sistema Frances
            var cuotaMensual = CalcularCuotaMensualSistemaFrances(request.MontoCapital, request.TasaInteres, request.PlazoMeses);
            var montoTotalConIntereses = CalcularMontoTotalConIntereses(request.MontoCapital, request.TasaInteres, request.PlazoMeses);
            var interesesTotales = montoTotalConIntereses - request.MontoCapital;

            //Crea el prestamo
            var loan = new Loan
            {
                NumeroPrestamo = numeroPrestamo,
                MontoCapital = request.MontoCapital,
                CuotasTotales = request.PlazoMeses,
                CuotasPagadas = 0,
                MontoPendiente = montoTotalConIntereses,
                TasaInteres = request.TasaInteres,
                PlazoMeses = request.PlazoMeses,
                IsActive = true,
                FechaCreacion = DateTime.UtcNow,
                UserId = request.UserId,
                AdminUserId = adminUserId,
                TablaAmortizacion = new List<LoanPaymentSchedule>()
            };

            var savedLoan = await _loanRepository.AddAsync(loan);

            //Genera la tabla de amortizacion
            var schedule = GeneratePaymentSchedule(savedLoan, cuotaMensual, request.MontoCapital, request.TasaInteres);
            await _scheduleRepository.AddRangeAsync(schedule);

            //Deposita el dinero en la cuenta principal del cliente
            var cuentaPrincipal = await _savingsAccountRepository.GetPrincipalByUserIdAsync(request.UserId);
            if (cuentaPrincipal == null)
            {
                return Result.Fail("El cliente no tiene una cuenta de ahorro principal.");
            }

            cuentaPrincipal.Balance += request.MontoCapital;
            await _savingsAccountRepository.UpdateAsync(cuentaPrincipal);

            //Registra la transaccion tipo CREDITO
            var transaccion = new Transaction
            {
                SavingsAccountId = cuentaPrincipal.Id,
                Monto = request.MontoCapital,
                FechaTransaccion = DateTime.UtcNow,
                Tipo = "CREDITO",
                Origen = numeroPrestamo,
                Beneficiario = cuentaPrincipal.NumeroCuenta,
                Estado = "APROBADA"
            };
            await _transactionRepository.AddAsync(transaccion);

            //Envia el correo al cliente
            await EnviarCorreoPrestamoAprobado(user.Email, user.Nombre, user.Apellido, numeroPrestamo, request.MontoCapital, request.PlazoMeses, request.TasaInteres, cuotaMensual);

            return Result.Ok();
        }

        public async Task<Result> UpdateLoanAsync(UpdateLoanDTO request, CancellationToken cancellationToken = default)
        {
            var loan = await _loanRepository.GetByIdWithScheduleAsync(request.Id);
            if (loan == null)
                return Result.Fail("Préstamo no encontrado");

            bool tasaCambiada = false;
            decimal nuevaTasa = loan.TasaInteres;

            if (request.TasaInteres.HasValue && request.TasaInteres.Value != loan.TasaInteres)
            {
                loan.TasaInteres = request.TasaInteres.Value;
                nuevaTasa = request.TasaInteres.Value;
                tasaCambiada = true;
            }

            if (request.MontoCapital.HasValue)
                loan.MontoCapital = request.MontoCapital.Value;

            if (request.PlazoMeses.HasValue)
            {
                if (request.PlazoMeses.Value % 6 != 0 || request.PlazoMeses.Value < 6 || request.PlazoMeses.Value > 60)
                    return Result.Fail("El plazo debe ser un múltiplo de 6 meses entre 6 y 60 meses");

                loan.PlazoMeses = request.PlazoMeses.Value;
            }

            // Si la tasa cambio, recalcular solo las cuotas futuras
            if (tasaCambiada)
            {
                var fechaActual = DateTime.UtcNow;
                var cuotasFuturas = loan.TablaAmortizacion
                    .Where(c => !c.Pagada && c.FechaPago > fechaActual)
                    .OrderBy(c => c.NumeroCuota)
                    .ToList();

                if (cuotasFuturas.Any())
                {
                    // Calcula saldo pendiente actual
                    var saldoPendienteActual = loan.TablaAmortizacion
                        .Where(c => !c.Pagada)
                        .Sum(c => c.ValorCuota);

                    var cuotasRestantes = cuotasFuturas.Count;
                    var nuevaCuotaMensual = CalcularCuotaMensualSistemaFrances(saldoPendienteActual, nuevaTasa, cuotasRestantes);

                    // Actualiza todas las cuotas futuras y recalcular saldos pendientes
                    var nuevoSaldoPendiente = saldoPendienteActual;
                    foreach (var cuota in cuotasFuturas.OrderBy(c => c.NumeroCuota))
                    {
                        cuota.ValorCuota = nuevaCuotaMensual;
                        nuevoSaldoPendiente -= nuevaCuotaMensual;
                        if (nuevoSaldoPendiente < 0) nuevoSaldoPendiente = 0;
                        cuota.SaldoPendiente = nuevoSaldoPendiente;
                    }

                    await _scheduleRepository.UpdateRangeAsync(cuotasFuturas);

                    // Actualiza el monto pendiente del prestamo
                    loan.MontoPendiente = loan.TablaAmortizacion
                        .Where(c => !c.Pagada)
                        .Sum(c => c.ValorCuota);
                }

                // Envia el correo al cliente sobre el cambio de tasa
                var user = await _identityUserManager.GetByIdAsync(loan.UserId);
                if (user != null)
                {
                    var primeraCuotaFutura = cuotasFuturas.FirstOrDefault();
                    decimal nuevaCuotaMensual = 0;
                    await EnviarCorreoTasaActualizada(user.Email, user.Nombre, user.Apellido, loan.NumeroPrestamo, nuevaTasa, nuevaCuotaMensual, primeraCuotaFutura?.FechaPago ?? DateTime.UtcNow);
                }
            }

            await _loanRepository.UpdateAsync(loan);
            return Result.Ok();
        }

        private string CalculatePaymentStatus(Loan loan)
        {
            if (!loan.IsActive)
                return "Completado";

            var schedule = loan.TablaAmortizacion?.OrderBy(s => s.NumeroCuota).ToList();
            if (schedule == null || !schedule.Any())
                return "Al día";

            var today = DateTime.UtcNow.Date;
            var cuotasAtrasadas = schedule
                .Where(s => !s.Pagada && s.FechaPago < today)
                .Any();

            return cuotasAtrasadas ? "En mora" : "Al día";
        }
        private List<LoanPaymentSchedule> GeneratePaymentSchedule(Loan loan, decimal cuotaMensual, decimal montoCapital, decimal tasaAnual)
        {
            var schedule = new List<LoanPaymentSchedule>();
            var fechaCreacion = loan.FechaCreacion;
            var saldoPendiente = loan.MontoPendiente;

            for (int i = 1; i <= loan.PlazoMeses; i++)
            {
                // La primera cuota vence el mismo dia del mes siguiente
                var fechaVencimiento = fechaCreacion.AddMonths(i);
                
                // Calcula el saldo pendiente despues de esta cuota
                saldoPendiente -= cuotaMensual;
                if (saldoPendiente < 0) saldoPendiente = 0;

                schedule.Add(new LoanPaymentSchedule
                {
                    LoanId = loan.Id,
                    NumeroCuota = i,
                    ValorCuota = cuotaMensual,
                    SaldoPendiente = saldoPendiente,
                    FechaPago = fechaVencimiento,
                    Pagada = false,
                    Atrasada = false
                });
            }

            return schedule;
        }

        private async Task EnviarCorreoPrestamoAprobado(string email, string nombre, string apellido, string numeroPrestamo, decimal monto, int plazoMeses, decimal tasaInteres, decimal cuotaMensual)
        {
            var mensaje = $@"
         Estimado {nombre} {apellido},

        Su préstamo ha sido aprobado exitosamente.

        Detalles:
        - Número de préstamo: {numeroPrestamo}
        - Monto aprobado: RD${monto:N2}
        - Plazo: {plazoMeses} meses
        - Tasa de interés: {tasaInteres}% anual
        - Cuota mensual: RD${cuotaMensual}

        El monto ha sido depositado en su cuenta de ahorro principal

        Gracias,
        Artemis Banking";

            var emailRequest = new EmailRequestDto
            {
                To = email,
                Subject = "Préstamo aprobado",
                Body = mensaje
            };

            await _emailService.SendAsync(emailRequest);
        }

        private async Task EnviarCorreoTasaActualizada(string email, string nombre, string apellido, string numeroPrestamo, decimal nuevaTasa, decimal nuevaCuota, DateTime fechaAplicacion)
        {
            var mensaje = $@"
        Estimado {nombre} {apellido},

        La tasa de interés de su préstamo #{numeroPrestamo} ha sido actualizada.

        - Nueva tasa: {nuevaTasa}% anual
        - Nueva cuota mensual: RD${nuevaCuota:N2}
        - Aplica desde: {fechaAplicacion:dd/MM/yyyy}

        Gracias,
        Artemis Banking";

            var emailRequest = new EmailRequestDto
            {
                To = email,
                Subject = "Tasa de interés actualizada",
                Body = mensaje
            };

            await _emailService.SendAsync(emailRequest);
        }
    }
}
