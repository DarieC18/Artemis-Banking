public class CashierThirdPartyTransferDTO
{
    public string SourceAccountNumber { get; set; } = string.Empty;
    public string DestinationAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string OperatedByUserId { get; set; } = string.Empty;
}
