namespace ArtemisBanking.Application.Dtos.Transaction
{
    public class CreateTransactionBeneficiaryDTO
    {
        public string CuentaOrigen { get; set; }
        public int BeneficiaryId { get; set; }
        public decimal Monto { get; set; }
        public string UserId { get; set; }
    }
}