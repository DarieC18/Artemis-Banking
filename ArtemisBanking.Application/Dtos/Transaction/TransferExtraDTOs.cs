namespace ArtemisBanking.Application.Dtos.Transaction
{
    public class TransferPreviewDTO
    {
        public string SourceAccountNumber { get; set; } = default!;
        public string SourceAccountMasked { get; set; } = default!;
        public decimal SourceCurrentBalance { get; set; }

        public string DestinationAccountNumber { get; set; } = default!;
        public string DestinationAccountMasked { get; set; } = default!;
        public string DestinationHolderFullName { get; set; } = default!;

        public decimal Amount { get; set; }
    }

    public class TransferResultDTO
    {
        public string SourceAccountNumber { get; set; } = default!;
        public string DestinationAccountNumber { get; set; } = default!;

        public decimal Amount { get; set; }
        public decimal NewSourceBalance { get; set; }
        public decimal NewDestinationBalance { get; set; }

        public DateTime ExecutedAt { get; set; }
        public string SourceHolderEmail { get; set; } = default!;
        public string DestinationHolderEmail { get; set; } = default!;
    }
}
