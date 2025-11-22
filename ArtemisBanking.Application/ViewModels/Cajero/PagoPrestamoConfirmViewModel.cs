using ArtemisBanking.Application.Dtos.Loan;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    public class PagoPrestamoConfirmViewModel
    {
        public string CuentaOrigen { get; set; } = string.Empty;
        public string CuentaOrigenEnmascarada { get; set; } = string.Empty;
        public decimal BalanceActualCuentaOrigen { get; set; }
        public int LoanId { get; set; }
        public string NumeroPrestamo { get; set; } = string.Empty;
        public string TitularPrestamo { get; set; } = string.Empty;
        public decimal DeudaPendienteActual { get; set; }
        public decimal MontoSolicitado { get; set; }
        public IReadOnlyCollection<LoanPaymentScheduleDTO> CuotasAfectadas { get; set; }
            = new List<LoanPaymentScheduleDTO>();

        public string? ErrorMessage { get; set; }
    }
}
