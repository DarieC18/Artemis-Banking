namespace ArtemisBanking.Application.Dtos.Transaction
{
    public class CashierThirdPartyTransferResultDTO
    {
        public string SourceAccountNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
