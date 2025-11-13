namespace ArtemisBanking.Domain.Entities
{
    public class LoanPaymentSchedule
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public int NumeroCuota { get; set; }
        public decimal ValorCuota { get; set; }
        public decimal SaldoPendiente { get; set; }
        public DateTime FechaPago { get; set; }
        public bool Pagada { get; set; }
        public bool Atrasada { get; set; }

        public virtual Loan Loan { get; set; }
    }
}