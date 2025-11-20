namespace ArtemisBanking.Application.Dtos.Transaction
{
    public class PayLoanDTO
    {
        public int LoanId { get; set; }
        public string LoanNumber { get; set; } = "";
        public string CuentaOrigen { get; set; } = "";
        public decimal Monto { get; set; }
        public string UserId { get; set; } = "";
    }
}
