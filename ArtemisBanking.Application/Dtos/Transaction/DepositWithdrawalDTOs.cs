namespace ArtemisBanking.Application.Dtos.Transaction
{
    // Deposito

    public class DepositDTO
    {
        public string AccountNumber { get; set; } = default!;
        public decimal Amount { get; set; }
        public string OperatedByUserId { get; set; } = default!;
    }

    public class DepositPreviewDTO
    {
        public string AccountNumber { get; set; } = default!;
        public string AccountMasked { get; set; } = default!;
        public string HolderFullName { get; set; } = default!;
        public decimal Amount { get; set; }
        public decimal CurrentBalance { get; set; }
    }

    public class DepositResultDTO
    {
        public string AccountNumber { get; set; } = default!;
        public decimal Amount { get; set; }
        public decimal NewBalance { get; set; }
        public DateTime ExecutedAt { get; set; }
        public string HolderEmail { get; set; } = default!;
    }

    // Retiro

    public class WithdrawalDTO
    {
        public string AccountNumber { get; set; } = default!;
        public decimal Amount { get; set; }
        public string OperatedByUserId { get; set; } = default!;
    }

    public class WithdrawalPreviewDTO
    {
        public string AccountNumber { get; set; } = default!;
        public string AccountMasked { get; set; } = default!;
        public string HolderFullName { get; set; } = default!;
        public decimal Amount { get; set; }
        public decimal CurrentBalance { get; set; }
    }

    public class WithdrawalResultDTO
    {
        public string AccountNumber { get; set; } = default!;
        public decimal Amount { get; set; }
        public decimal NewBalance { get; set; }
        public DateTime ExecutedAt { get; set; }
        public string HolderEmail { get; set; } = default!;
    }
}
