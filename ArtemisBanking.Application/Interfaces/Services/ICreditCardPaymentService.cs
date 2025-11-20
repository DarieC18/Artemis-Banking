using ArtemisBanking.Application.Dtos.Transaction;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface ICreditCardPaymentService
    {
        Task<Result<PayCreditCardPreviewDTO>> PreviewPayCreditCardAsync(PayCreditCardDTO request);
        Task<Result<PayCreditCardResultDTO>> ExecutePayCreditCardAsync(PayCreditCardDTO command);
    }
}
