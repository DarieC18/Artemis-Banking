using ArtemisBanking.Domain.Enums;

namespace ArtemisBanking.Application.Dtos.Cajero
{
    public class CajeroHomeDashboardDTO
    {
        public DateOnly Date { get; set; }

        public int TotalTransactions { get; set; }
        public int TotalDeposits { get; set; }
        public int TotalWithdrawals { get; set; }
        public int TotalPayments { get; set; }
        public int TotalThirdPartyTransfers { get; set; }

        public IReadOnlyCollection<CajeroTransactionSummaryDTO> RecentTransactions { get; set; }
            = Array.Empty<CajeroTransactionSummaryDTO>();
    }

    public class CajeroTransactionSummaryDTO
    {
        public DateTime ExecutedAt { get; set; }
        public TransactionOperationType OperationType { get; set; }
        public string SourceAccountMasked { get; set; } = default!;
        public string? DestinationOrReference { get; set; }
        public decimal Amount { get; set; }
    }
}
