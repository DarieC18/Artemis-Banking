namespace ArtemisBanking.Application.Dtos.Loan
{
    public class LoanDTO
    {
        public int Id { get; set; }
        public string NumeroPrestamo { get; set; }
        public decimal MontoCapital { get; set; }
        public int CuotasTotales { get; set; }
        public int CuotasPagadas { get; set; }
        public decimal MontoPendiente { get; set; }
        public decimal TasaInteres { get; set; }
        public int PlazoMeses { get; set; }
        public string EstadoPago { get; set; }  // "Al dia" o "En mora"
    }
}