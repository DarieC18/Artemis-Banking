using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels.Cliente;
using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Services
{
    public class ClienteProductoService : IClienteProductoService
    {
        private readonly ISavingsAccountReadRepository _savingsAccountRepository;
        private readonly ILoanReadRepository _loanReadRepository;
        private readonly ICreditCardReadRepository _creditCardReadRepository;

        public ClienteProductoService(
            ISavingsAccountReadRepository savingsAccountRepository,
            ILoanReadRepository loanReadRepository,
            ICreditCardReadRepository creditCardReadRepository)
        {
            _savingsAccountRepository = savingsAccountRepository;
            _loanReadRepository = loanReadRepository;
            _creditCardReadRepository = creditCardReadRepository;
        }

        public async Task<DetalleCuentaViewModel?> GetDetalleCuentaAsync(string userId, int cuentaId)
        {
            var cuenta = await _savingsAccountRepository
                .GetByIdWithTransactionsAsync(cuentaId, userId);

            if (cuenta == null)
                return null;

            var vm = new DetalleCuentaViewModel
            {
                CuentaId = cuenta.Id,
                NumeroCuenta = cuenta.NumeroCuenta,
                BalanceActual = cuenta.Balance,
                EsPrincipal = cuenta.EsPrincipal
            };

            var transaccionesOrdenadas = cuenta.Transactions
                .OrderByDescending(t => t.FechaTransaccion);

            foreach (var t in transaccionesOrdenadas)
            {
                vm.Transacciones.Add(new TransaccionDetalleViewModel
                {
                    Fecha = t.FechaTransaccion,
                    Monto = t.Monto,
                    Tipo = ObtenerTipo(t),
                    Beneficiario = ObtenerBeneficiario(t),
                    Origen = ObtenerOrigen(t),
                    Estado = t.Estado
                });
            }

            return vm;
        }
        private string ObtenerTipo(Transaction t)
        {
            if (!string.IsNullOrWhiteSpace(t.Tipo))
                return t.Tipo.ToUpperInvariant();

            if (t.Monto < 0)
                return "DÉBITO";

            return "CRÉDITO";
        }

        private string ObtenerBeneficiario(Transaction t)
        {
            return string.IsNullOrWhiteSpace(t.Beneficiario)
                ? "N/A"
                : t.Beneficiario;
        }

        private string ObtenerOrigen(Transaction t)
        {
            return string.IsNullOrWhiteSpace(t.Origen)
                ? "N/A"
                : t.Origen;
        }
        public async Task<DetallePrestamoViewModel?> GetDetallePrestamoAsync(string userId, int prestamoId)
        {
            var loan = await _loanReadRepository.GetByIdWithScheduleAsync(prestamoId, userId);

            if (loan == null)
                return null;

            var hoy = DateTime.UtcNow.Date;

            var cuotasOrdenadas = loan.TablaAmortizacion
                .OrderBy(c => c.FechaPago)
                .ToList();

            bool tieneCuotaAtrasada = cuotasOrdenadas
                .Any(c => !c.Pagada && c.FechaPago.Date < hoy);

            var vm = new DetallePrestamoViewModel
            {
                PrestamoId = loan.Id,
                NumeroPrestamo = loan.NumeroPrestamo,
                MontoCapital = loan.MontoCapital,
                CuotasTotales = loan.CuotasTotales,
                CuotasPagadas = loan.CuotasPagadas,
                MontoPendiente = loan.MontoPendiente,
                TasaInteres = loan.TasaInteres,
                PlazoMeses = loan.PlazoMeses,
                EstadoPago = tieneCuotaAtrasada ? "En mora" : "Al dia"
            };

            foreach (var c in cuotasOrdenadas)
            {
                vm.Cuotas.Add(new CuotaPrestamoViewModel
                {
                    NumeroCuota = c.NumeroCuota,
                    FechaPago = c.FechaPago,
                    ValorCuota = c.ValorCuota,
                    SaldoPendiente = c.SaldoPendiente,
                    Pagada = c.Pagada,
                    Atrasada = !c.Pagada && c.FechaPago.Date < hoy
                });
            }
            return vm;
        }
        public async Task<DetalleTarjetaViewModel?> GetDetalleTarjetaAsync(string userId, int tarjetaId)
        {
            var card = await _creditCardReadRepository.GetByIdWithConsumptionsAsync(tarjetaId, userId);

            if (card == null)
                return null;

            var vm = new DetalleTarjetaViewModel
            {
                TarjetaId = card.Id,
                NumeroTarjeta = card.NumeroTarjeta,
                Ultimos4Digitos = card.NumeroTarjeta?.Substring(card.NumeroTarjeta.Length - 4) ?? "",
                LimiteCredito = card.LimiteCredito,
                DeudaActual = card.DeudaActual,
                CreditoDisponible = card.LimiteCredito - card.DeudaActual,
                FechaExpiracion = card.FechaExpiracion,
                FechaCreacion = card.FechaCreacion
            };

            var consumosOrdenados = card.Consumos
                .OrderByDescending(c => c.FechaConsumo)
                .ToList();

            foreach (var c in consumosOrdenados)
            {
                vm.Consumos.Add(new ConsumoTarjetaViewModel
                {
                    FechaConsumo = c.FechaConsumo,
                    Monto = c.Monto,
                    Comercio = ObtenerNombreComercio(c),
                    Estado = c.Estado
                });
            }

            return vm;
        }

        private string ObtenerNombreComercio(CreditCardConsumption c)
        {
            if (string.IsNullOrWhiteSpace(c.Comercio))
                return "AVANCE";

            return c.Comercio;
        }
    }
}
