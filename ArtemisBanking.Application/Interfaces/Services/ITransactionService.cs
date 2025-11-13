using ArtemisBanking.Application.ViewModels;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface ITransactionService
    {
        Task ExecuteTransactionExpressAsync(string userId, TransactionExpressViewModel model);
        Task ExecuteTransactionBeneficiaryAsync(string userId, TransactionBeneficiaryViewModel model);
        Task ExecutePayCreditCardAsync(string userId, PayCreditCardViewModel model);
        Task ExecutePayLoanAsync(string userId, PayLoanViewModel model);
        Task ExecuteCashAdvanceAsync(string userId, CashAdvanceViewModel model);
        Task ExecuteTransferBetweenAccountsAsync(string userId, TransferBetweenAccountsViewModel model);
    }
}
