namespace ArtemisBanking.Application.Dtos.Transaction
{
    public class CreditCardPaymentPreviewDTO
    {
        public string CreditCardNumberMasked { get; set; } = null!;
        public string HolderFullName { get; set; } = null!;
        public decimal CurrentDebt { get; set; }
        public decimal AmountToPay { get; set; }
        public decimal NewDebt { get; set; }
        public decimal CurrentLimit { get; set; }
        public decimal CurrentAvailableLimit { get; set; }
        public decimal NewAvailableLimit { get; set; }
        public string OperationType { get; set; } = null!;
        public DateTime OperationDate { get; set; }
        public string TellerUserName { get; set; } = null!;
    }
}
