using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Application.Dtos.Transaction;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface ITransactionService
    {
        Task CreateTransactionExpressAsync(string userId, CreateTransactionExpressDTO dto);
        Task CreateTransactionToBeneficiaryAsync(string userId, CreateTransactionBeneficiaryDTO dto);
        Task TransferBetweenAccountsAsync(string userId, TransferBetweenAccountsDTO dto);
        Task PayLoanAsync(string userId, PayLoanDTO dto);
        Task PayCreditCardAsync(string userId, PayCreditCardDTO dto);
        Task CreateCashAdvanceAsync(string userId, CashAdvanceDTO dto);
    }
}
