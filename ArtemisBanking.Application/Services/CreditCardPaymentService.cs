using ArtemisBanking.Application.Dtos.Transaction;
using ArtemisBanking.Application.DTOs.Email;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Enums;

namespace ArtemisBanking.Application.Services
{
    public class CreditCardPaymentService : ICreditCardPaymentService
    {
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IEmailService _emailService;
        private readonly IUserInfoService _userInfoService;

        public CreditCardPaymentService(
            ISavingsAccountRepository savingsAccountRepository,
            ICreditCardRepository creditCardRepository,
            ITransactionRepository transactionRepository,
            IEmailService emailService,
            IUserInfoService userInfoService)
        {
            _savingsAccountRepository = savingsAccountRepository;
            _creditCardRepository = creditCardRepository;
            _transactionRepository = transactionRepository;
            _emailService = emailService;
            _userInfoService = userInfoService;
        }

        public async Task<Result<PayCreditCardPreviewDTO>> PreviewPayCreditCardAsync(PayCreditCardDTO request)
        {
            if (request.Monto <= 0)
                return Result<PayCreditCardPreviewDTO>.Fail("El monto a pagar debe ser mayor que cero.");

            if (string.IsNullOrWhiteSpace(request.CuentaOrigen))
                return Result<PayCreditCardPreviewDTO>.Fail("Debe especificar la cuenta de origen del pago.");

            var sourceAccount = await _savingsAccountRepository
                .GetByAccountNumberAsync(request.CuentaOrigen);

            if (sourceAccount is null)
                return Result<PayCreditCardPreviewDTO>.Fail("La cuenta origen no existe.");

            if (!sourceAccount.IsActive)
                return Result<PayCreditCardPreviewDTO>.Fail("La cuenta origen está inactiva.");

            if (sourceAccount.Balance < request.Monto)
                return Result<PayCreditCardPreviewDTO>.Fail("Fondos insuficientes en la cuenta de origen.");

            var creditCard = await _creditCardRepository.GetByNumberAsync(request.CardNumber);

            if (creditCard is null)
                return Result<PayCreditCardPreviewDTO>.Fail("La tarjeta no existe o está inactiva.");

            request.CreditCardId = creditCard.Id;

            if (!creditCard.IsActive)
                return Result<PayCreditCardPreviewDTO>.Fail("La tarjeta de crédito está inactiva.");

            if (creditCard.DeudaActual <= 0)
                return Result<PayCreditCardPreviewDTO>.Fail("La tarjeta de crédito no tiene deuda pendiente.");

            var realPaymentAmount = Math.Min(request.Monto, creditCard.DeudaActual);

            var cardHolder = await _userInfoService.GetUserBasicInfoByIdAsync(creditCard.UserId);
            var holderFullName = cardHolder is null
                ? "Cliente"
                : $"{cardHolder.Nombre} {cardHolder.Apellido}".Trim();

            var preview = new PayCreditCardPreviewDTO
            {
                SourceAccountNumber = sourceAccount.NumeroCuenta,
                SourceAccountMasked = MaskAccountNumber(sourceAccount.NumeroCuenta),
                SourceCurrentBalance = sourceAccount.Balance,

                CardNumberMasked = MaskCardNumber(creditCard.NumeroTarjeta),
                CardHolderFullName = holderFullName,
                CurrentDebt = creditCard.DeudaActual,

                RequestedAmount = request.Monto,
                RealPaymentAmount = realPaymentAmount,

                InternalCardId = creditCard.Id
            };

            return Result<PayCreditCardPreviewDTO>.Ok(preview);
        }

        public async Task<Result<PayCreditCardResultDTO>> ExecutePayCreditCardAsync(PayCreditCardDTO command)
        {
            if (command.Monto <= 0)
                return Result<PayCreditCardResultDTO>.Fail("El monto a pagar debe ser mayor que cero.");

            if (string.IsNullOrWhiteSpace(command.CuentaOrigen))
                return Result<PayCreditCardResultDTO>.Fail("Debe especificar la cuenta de origen del pago.");

            var now = DateTime.Now;

            var sourceAccount = await _savingsAccountRepository
                .GetByAccountNumberAsync(command.CuentaOrigen);

            if (sourceAccount is null)
                return Result<PayCreditCardResultDTO>.Fail("La cuenta origen no existe.");

            if (!sourceAccount.IsActive)
                return Result<PayCreditCardResultDTO>.Fail("La cuenta origen está inactiva.");

            if (sourceAccount.Balance < command.Monto)
                return Result<PayCreditCardResultDTO>.Fail("Fondos insuficientes en la cuenta de origen.");

            var creditCard = await _creditCardRepository.GetByIdAsync(command.CreditCardId);

            if (creditCard is null)
                return Result<PayCreditCardResultDTO>.Fail("La tarjeta de crédito no existe.");

            if (!creditCard.IsActive)
                return Result<PayCreditCardResultDTO>.Fail("La tarjeta de crédito está inactiva.");

            if (creditCard.DeudaActual <= 0)
                return Result<PayCreditCardResultDTO>.Fail("La tarjeta de crédito no tiene deuda pendiente.");

            var realPaymentAmount = Math.Min(command.Monto, creditCard.DeudaActual);

            if (realPaymentAmount <= 0)
                return Result<PayCreditCardResultDTO>.Fail("El monto indicado no aplica a ninguna parte de la deuda.");

            sourceAccount.Balance -= realPaymentAmount;

            creditCard.DeudaActual -= realPaymentAmount;

            await _savingsAccountRepository.UpdateAsync(sourceAccount);
            await _creditCardRepository.UpdateAsync(creditCard);

            var transaction = new Transaction
            {
                SavingsAccountId = sourceAccount.Id,
                Monto = realPaymentAmount,
                FechaTransaccion = now,
                Tipo = "DÉBITO",
                Origen = sourceAccount.NumeroCuenta,
                Beneficiario = creditCard.NumeroTarjeta,
                Estado = "APROBADA",
                OperationType = TransactionOperationType.PagoTarjetaCredito,
                OperatedByUserId = command.UserId
            };

            await _transactionRepository.AddAsync(transaction);

            var cardHolder = await _userInfoService.GetUserBasicInfoByIdAsync(creditCard.UserId);
            var email = cardHolder?.Email ?? string.Empty;

            var resultDto = new PayCreditCardResultDTO
            {
                SourceAccountNumber = sourceAccount.NumeroCuenta,
                CardNumberMasked = MaskCardNumber(creditCard.NumeroTarjeta),
                PaidAmount = realPaymentAmount,
                NewCardDebt = creditCard.DeudaActual,
                NewSourceAccountBalance = sourceAccount.Balance,
                ExecutedAt = now,
                CardHolderEmail = email
            };

            var subject = $"Pago realizado a la tarjeta {resultDto.CardNumberMasked}";

            var body = $@"
            <html>
              <body style=""font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; padding: 20px;"">
                <table width=""100%"" cellpadding=""0"" cellspacing=""0""
                       style=""max-width: 600px; margin: auto; background: #ffffff; border-radius: 10px;
                              padding: 25px; border: 1px solid #e0e0e0;"">
                  <tr>
                    <td>

                      <h2 style=""color: #2e2e2e; margin-bottom: 15px;"">
                        Notificación de pago de tarjeta de crédito
                      </h2>

                      <p style=""font-size: 15px; margin-bottom: 10px;"">
                        Hola,
                      </p>

                      <p style=""font-size: 15px; line-height: 1.5;"">
                        Se ha realizado un pago a su tarjeta de crédito.
                      </p>

                      <p style=""margin-top: 20px; font-size: 15px;"">
                        <strong>Monto pagado:</strong> {realPaymentAmount:C}<br/>
                        <strong>Tarjeta:</strong> {resultDto.CardNumberMasked}<br/>
                        <strong>Cuenta origen:</strong> {MaskAccountNumber(sourceAccount.NumeroCuenta)}<br/>
                        <strong>Fecha y hora:</strong> {now:dd/MM/yyyy HH:mm}
                      </p>

                      <p style=""margin-top: 25px; font-size: 15px;"">
                        <strong>Deuda restante de la tarjeta:</strong> {creditCard.DeudaActual:C}<br/>
                        <strong>Nuevo balance de la cuenta de origen:</strong> {sourceAccount.Balance:C}
                      </p>

                      <div style=""margin-top: 25px; padding: 15px; background-color: #fff4e5;
                                  border-left: 4px solid #ffa726; font-size: 14px;"">
                        Si usted no reconoce esta operación, contacte al banco de inmediato.
                      </div>

                      <p style=""margin-top: 30px; color: #888; font-size: 12px;"">
                        ArtemisBanking © {DateTime.Now.Year}
                      </p>

                    </td>
                  </tr>
                </table>
              </body>
            </html>
            ";

            var emailRequest = new EmailRequestDto
            {
                To = email,
                Subject = subject,
                Body = body
            };

            await _emailService.SendAsync(emailRequest);

            return Result<PayCreditCardResultDTO>.Ok(resultDto);
        }

        private static string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length < 4)
                return accountNumber;

            var last4 = accountNumber[^4..];
            return $"****{last4}";
        }

        private static string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
                return cardNumber;

            var last4 = cardNumber[^4..];
            return $"****{last4}";
        }
    }
}
