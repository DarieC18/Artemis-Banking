namespace ArtemisBanking.Domain.Entities
{
    public class Loan
    {
        public int Id { get; set; }
        public string NumeroPrestamo { get; set; }
        public decimal MontoCapital { get; set; }
        public int CuotasTotales { get; set; }
        public int CuotasPagadas { get; set; }
        public decimal MontoPendiente { get; set; }
        public decimal TasaInteres { get; set; }
        public int PlazoMeses { get; set; }
        public bool IsActive { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UserId { get; set; }
        public string AdminUserId { get; set; }
        public virtual ICollection<LoanPaymentSchedule> TablaAmortizacion { get; set; }
            = new List<LoanPaymentSchedule>();

    }
}