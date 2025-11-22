namespace ArtemisBanking.Application.Dtos.CreditCard
{
    public class CashAdvanceDTO
    {
        public int CreditCardId { get; set; }
        public string CuentaDestino { get; set; }
        public decimal Monto { get; set; }
        public string UserId { get; set; }
    }
}