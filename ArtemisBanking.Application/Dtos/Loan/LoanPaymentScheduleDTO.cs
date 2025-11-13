namespace ArtemisBanking.Application.Dtos.Loan
{
    public class LoanPaymentScheduleDTO
    {
        public int NumeroCuota { get; set; }
        public decimal ValorCuota { get; set; }
        public DateTime FechaPago { get; set; }
        public bool Pagada { get; set; }
        public bool Atrasada { get; set; }
    }
}