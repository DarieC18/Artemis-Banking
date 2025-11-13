namespace ArtemisBanking.Application.Dtos.Loan
{
    public class LoanDetailDTO
    {
        public string NumeroPrestamo { get; set; }
        public decimal MontoCapital { get; set; }
        public int CuotasTotales { get; set; }
        public int CuotasPagadas { get; set; }
        public decimal MontoPendiente { get; set; }
        public decimal TasaInteres { get; set; }
        public int PlazoMeses { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string EstadoPago { get; set; }
        public List<LoanPaymentScheduleDTO> TablaAmortizacion { get; set; }
    }
}