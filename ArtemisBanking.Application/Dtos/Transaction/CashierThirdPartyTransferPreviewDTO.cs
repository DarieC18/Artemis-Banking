namespace ArtemisBanking.Application.Dtos.Transaction
{
    public class CashierThirdPartyTransferPreviewDTO
    {
        public string SourceAccountNumber { get; set; } = string.Empty;
        public string SourceAccountMasked { get; set; } = string.Empty;
        public decimal SourceCurrentBalance { get; set; }

        public string DestinationAccountNumber { get; set; } = string.Empty;
        public string DestinationHolderFullName { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }
}
