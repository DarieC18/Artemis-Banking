using ArtemisBanking.Application.Dtos.Transaction;
using ArtemisBanking.Application.DTOs.Email;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Enums;

namespace ArtemisBanking.Application.Services
{
    public class CashierThirdPartyTransferService : ICashierThirdPartyTransferService
    {
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly IEmailService _emailService;

        public CashierThirdPartyTransferService(
            ISavingsAccountRepository savingsAccountRepository,
            ITransactionRepository transactionRepository,
            IUserInfoService userInfoService,
            IEmailService emailService)
        {
            _savingsAccountRepository = savingsAccountRepository;
            _transactionRepository = transactionRepository;
            _userInfoService = userInfoService;
            _emailService = emailService;
        }

        public async Task<Result<CashierThirdPartyTransferPreviewDTO>> PreviewAsync(CashierThirdPartyTransferDTO dto)
        {
            if (dto.Amount <= 0)
                return Result<CashierThirdPartyTransferPreviewDTO>.Fail("El monto debe ser mayor que cero.");

            if (dto.SourceAccountNumber == dto.DestinationAccountNumber)
                return Result<CashierThirdPartyTransferPreviewDTO>
                    .Fail("La cuenta origen y la cuenta destino no pueden ser la misma.");

            var source = await _savingsAccountRepository.GetByAccountNumberAsync(dto.SourceAccountNumber);
            if (source is null || !source.IsActive)
                return Result<CashierThirdPartyTransferPreviewDTO>
                    .Fail("La cuenta origen no existe o está inactiva.");

            var destination = await _savingsAccountRepository.GetByAccountNumberAsync(dto.DestinationAccountNumber);
            if (destination is null || !destination.IsActive)
                return Result<CashierThirdPartyTransferPreviewDTO>
                    .Fail("La cuenta destino no existe o está inactiva.");

            if (source.Balance < dto.Amount)
                return Result<CashierThirdPartyTransferPreviewDTO>
                    .Fail("La cuenta origen no tiene fondos suficientes.");

            var user = await _userInfoService.GetUserBasicInfoByIdAsync(destination.UserId);
            var fullName = user is null ? "Cliente" : $"{user.Nombre} {user.Apellido}".Trim();

            var preview = new CashierThirdPartyTransferPreviewDTO
            {
                SourceAccountNumber = source.NumeroCuenta,
                SourceAccountMasked = Mask(source.NumeroCuenta),
                SourceCurrentBalance = source.Balance,

                DestinationAccountNumber = destination.NumeroCuenta,
                DestinationHolderFullName = fullName,

                Amount = dto.Amount
            };

            return Result<CashierThirdPartyTransferPreviewDTO>.Ok(preview);
        }

        public async Task<Result<CashierThirdPartyTransferResultDTO>> ExecuteAsync(CashierThirdPartyTransferDTO dto)
        {
            var previewResult = await PreviewAsync(dto);
            if (previewResult.IsFailure)
                return Result<CashierThirdPartyTransferResultDTO>.Fail(previewResult.GeneralError);

            var now = DateTime.Now;

            var source = await _savingsAccountRepository.GetByAccountNumberAsync(dto.SourceAccountNumber);
            var destination = await _savingsAccountRepository.GetByAccountNumberAsync(dto.DestinationAccountNumber);

            if (source is null || destination is null)
                return Result<CashierThirdPartyTransferResultDTO>.Fail("Ocurrió un error inesperado.");

            source.Balance -= dto.Amount;
            destination.Balance += dto.Amount;

            await _savingsAccountRepository.UpdateAsync(source);
            await _savingsAccountRepository.UpdateAsync(destination);

            var debitTx = new Transaction
            {
                SavingsAccountId = source.Id,
                Monto = dto.Amount,
                FechaTransaccion = now,
                Tipo = "DÉBITO",
                Origen = source.NumeroCuenta,
                Beneficiario = destination.NumeroCuenta,
                Estado = "APROBADA",
                OperationType = TransactionOperationType.CashierThirdPartyTransfer,
                OperatedByUserId = dto.OperatedByUserId
            };

            var creditTx = new Transaction
            {
                SavingsAccountId = destination.Id,
                Monto = dto.Amount,
                FechaTransaccion = now,
                Tipo = "CRÉDITO",
                Origen = source.NumeroCuenta,
                Beneficiario = destination.NumeroCuenta,
                Estado = "APROBADA",
                OperationType = TransactionOperationType.CashierThirdPartyTransfer,
                OperatedByUserId = dto.OperatedByUserId
            };

            await _transactionRepository.AddAsync(debitTx);
            await _transactionRepository.AddAsync(creditTx);

            var sourceUser = await _userInfoService.GetUserBasicInfoByIdAsync(source.UserId);
            var destinationUser = await _userInfoService.GetUserBasicInfoByIdAsync(destination.UserId);

            var emailSource = sourceUser?.Email ?? string.Empty;
            var emailDestination = destinationUser?.Email ?? string.Empty;

            var maskSrc4 = Last4(source.NumeroCuenta);
            var maskDst4 = Last4(destination.NumeroCuenta);

            await _emailService.SendAsync(new EmailRequestDto
            {
                To = emailSource,
                Subject = $"Transacción realizada a la cuenta {maskDst4}",
                Body =
                    $"""
                    Hola {sourceUser?.Nombre},

                    Se ha realizado una transferencia desde su cuenta.

                    Monto: {dto.Amount:C}
                    Cuenta destino: ****{maskDst4}
                    Fecha: {now:dd/MM/yyyy HH:mm}

                    Si no reconoce esta operación, contacte al banco de inmediato.

                    ArtemisBanking
                    """
            });

            await _emailService.SendAsync(new EmailRequestDto
            {
                To = emailDestination,
                Subject = $"Transacción recibida desde la cuenta {maskSrc4}",
                Body =
                    $"""
                    Hola {destinationUser?.Nombre},

                    Ha recibido una transferencia.

                    Monto: {dto.Amount:C}
                    Cuenta origen: ****{maskSrc4}
                    Fecha: {now:dd/MM/yyyy HH:mm}

                    ArtemisBanking
                    """
            });

            var result = new CashierThirdPartyTransferResultDTO
            {
                SourceAccountNumber = source.NumeroCuenta,
                DestinationAccountNumber = destination.NumeroCuenta,
                Amount = dto.Amount,
                Date = now
            };

            return Result<CashierThirdPartyTransferResultDTO>.Ok(result);
        }

        // Helpers
        private static string Mask(string account)
        {
            if (account.Length < 4) return account;
            return $"****{account[^4..]}";
        }

        private static string Last4(string account)
        {
            if (account.Length < 4) return account;
            return account[^4..];
        }
    }
}
