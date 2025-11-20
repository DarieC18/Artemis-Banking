namespace ArtemisBanking.Application.Dtos.Transaction
{
    public class PayCreditCardPreviewDTO
    {
        public string SourceAccountNumber { get; set; } = default!;
        public string SourceAccountMasked { get; set; } = default!;
        public decimal SourceCurrentBalance { get; set; }

        public string CardNumberMasked { get; set; } = default!;
        public string CardHolderFullName { get; set; } = default!;
        public decimal CurrentDebt { get; set; }
        public int InternalCardId { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal RealPaymentAmount { get; set; }
    }

    public class PayCreditCardResultDTO
    {
        public string SourceAccountNumber { get; set; } = default!;
        public string CardNumberMasked { get; set; } = default!;

        public decimal PaidAmount { get; set; }
        public decimal NewCardDebt { get; set; }
        public decimal NewSourceAccountBalance { get; set; }

        public DateTime ExecutedAt { get; set; }
        public string CardHolderEmail { get; set; } = default!;
    }
}
