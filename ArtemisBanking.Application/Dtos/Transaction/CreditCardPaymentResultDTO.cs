namespace ArtemisBanking.Application.Dtos.Transaction
{
    public class CreditCardPaymentResultDTO
    {
        public Guid TransactionId { get; set; }
        public string TransactionNumber { get; set; } = null!;
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
    }
}
