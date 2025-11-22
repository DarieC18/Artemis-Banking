using ArtemisBanking.Application.Dtos.Loan;

namespace ArtemisBanking.Application.Dtos.Transaction
{
    public class PayLoanPreviewDTO
    {
        public string SourceAccountNumber { get; set; } = default!;
        public string SourceAccountMasked { get; set; } = default!;
        public decimal SourceCurrentBalance { get; set; }
        public string LoanNumber { get; set; } = default!;
        public string LoanHolderFullName { get; set; } = default!;
        public decimal TotalDebtRemaining { get; set; }
        public int InternalLoanId { get; set; }
        public decimal RequestedAmount { get; set; }

        public IReadOnlyCollection<LoanPaymentScheduleDTO> InstallmentsToAffect { get; set; }
            = Array.Empty<LoanPaymentScheduleDTO>();
    }

    public class PayLoanResultDTO
    {
        public string LoanNumber { get; set; } = default!;
        public decimal PaidAmount { get; set; }
        public decimal NewTotalDebtRemaining { get; set; }
        public decimal NewSourceAccountBalance { get; set; }

        public bool HasChangeReturnedToSource { get; set; }
        public decimal ChangeAmount { get; set; }

        public DateTime ExecutedAt { get; set; }
        public string LoanHolderEmail { get; set; } = default!;
    }
}
