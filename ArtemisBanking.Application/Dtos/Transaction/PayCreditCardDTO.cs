namespace ArtemisBanking.Application.Dtos.Transaction
{
    public class PayCreditCardDTO
    {
        public int CreditCardId { get; set; }
        public string CuentaOrigen { get; set; }
        public decimal Monto { get; set; }
        public string UserId { get; set; }
    }
}