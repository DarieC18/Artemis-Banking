public class CashierThirdPartyTransferValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public string? SourceAccountNumber { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public string? DestinationCustomerFullName { get; set; }
    public decimal Amount { get; set; }
    public List<string> Errors { get; set; } = new();
}
