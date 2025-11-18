using ArtemisBanking.Application.Dtos.Transaction;
using ArtemisBanking.Application.DTOs.Email;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Enums;

namespace ArtemisBanking.Application.Services
{
    public class AccountCashOperationsService : IAccountCashOperationsService
    {
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IEmailService _emailService;
        private readonly IUserInfoService _userInfoService;

        public AccountCashOperationsService(
            ISavingsAccountRepository savingsAccountRepository,
            ITransactionRepository transactionRepository,
            IEmailService emailService,
            IUserInfoService userInfoService)
        {
            _savingsAccountRepository = savingsAccountRepository;
            _transactionRepository = transactionRepository;
            _emailService = emailService;
            _userInfoService = userInfoService;
        }

        public async Task<Result<DepositPreviewDTO>> PreviewDepositAsync(DepositDTO request)
        {
            if (request.Amount <= 0)
                return Result<DepositPreviewDTO>.Fail("El monto a depositar debe ser mayor que cero.");

            var account = await _savingsAccountRepository
                .GetByAccountNumberAsync(request.AccountNumber);

            if (account is null)
                return Result<DepositPreviewDTO>.Fail("La cuenta destino no existe.");

            if (!account.IsActive)
                return Result<DepositPreviewDTO>.Fail("La cuenta destino está inactiva.");

            var user = await _userInfoService.GetUserBasicInfoByIdAsync(account.UserId);
            var holderFullName = user is null
                ? "Cliente"
                : $"{user.Nombre} {user.Apellido}".Trim();


            var preview = new DepositPreviewDTO
            {
                AccountNumber = account.NumeroCuenta,
                AccountMasked = MaskAccountNumber(account.NumeroCuenta),
                HolderFullName = holderFullName,
                Amount = request.Amount,
                CurrentBalance = account.Balance
            };

            return Result<DepositPreviewDTO>.Ok(preview);
        }

        public async Task<Result<DepositResultDTO>> ExecuteDepositAsync(DepositDTO command)
        {
            if (command.Amount <= 0)
                return Result<DepositResultDTO>.Fail("El monto a depositar debe ser mayor que cero.");

            var now = DateTime.Now;

            var account = await _savingsAccountRepository
                .GetByAccountNumberAsync(command.AccountNumber);

            if (account is null)
                return Result<DepositResultDTO>.Fail("La cuenta destino no existe.");

            if (!account.IsActive)
                return Result<DepositResultDTO>.Fail("La cuenta destino está inactiva.");

            account.Balance += command.Amount;

            var transaction = new Transaction
            {
                SavingsAccountId = account.Id,
                Monto = command.Amount,
                FechaTransaccion = now,
                Tipo = "CRÉDITO",
                Origen = "DEPÓSITO",
                Beneficiario = account.NumeroCuenta,
                Estado = "APROBADA",
                OperationType = TransactionOperationType.Deposito,
                OperatedByUserId = command.OperatedByUserId
            };

            await _transactionRepository.AddAsync(transaction);
            await _savingsAccountRepository.UpdateAsync(account);

            var user = await _userInfoService.GetUserBasicInfoByIdAsync(account.UserId);
            var email = user?.Email ?? string.Empty;

            var result = new DepositResultDTO
            {
                AccountNumber = account.NumeroCuenta,
                Amount = command.Amount,
                NewBalance = account.Balance,
                ExecutedAt = now,
                HolderEmail = email
            };

            var subject = $"Depósito realizado a la cuenta {MaskAccountNumber(account.NumeroCuenta)}";
            var body = $"""
                Hola,

                Se ha realizado un depósito a su cuenta de ahorro.

                Monto: {command.Amount:C}
                Cuenta: {account.NumeroCuenta}
                Fecha y hora: {now:dd/MM/yyyy HH:mm}

                Si usted no reconoce esta operación, contacte al banco de inmediato.

                ArtemisBanking
                """;

            var emailRequest = new EmailRequestDto
            {
                To = email,
                Subject = subject,
                Body = body
            };

            await _emailService.SendAsync(emailRequest);

            return Result<DepositResultDTO>.Ok(result);
        }

        private static string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length < 4)
                return accountNumber;

            var last4 = accountNumber[^4..];
            return $"****{last4}";
        }
        public async Task<Result<WithdrawalPreviewDTO>> PreviewWithdrawalAsync(WithdrawalDTO request)
        {
            if (request.Amount <= 0)
                return Result<WithdrawalPreviewDTO>.Fail("El monto a retirar debe ser mayor que cero.");

            var account = await _savingsAccountRepository
                .GetByAccountNumberAsync(request.AccountNumber);

            if (account is null)
                return Result<WithdrawalPreviewDTO>.Fail("La cuenta origen no existe.");

            if (!account.IsActive)
                return Result<WithdrawalPreviewDTO>.Fail("La cuenta origen está inactiva.");

            if (account.Balance < request.Amount)
                return Result<WithdrawalPreviewDTO>.Fail("Fondos insuficientes en la cuenta.");

            var user = await _userInfoService.GetUserBasicInfoByIdAsync(account.UserId);
            var holderFullName = user is null
                ? "Cliente"
                : $"{user.Nombre} {user.Apellido}".Trim();


            var preview = new WithdrawalPreviewDTO
            {
                AccountNumber = account.NumeroCuenta,
                AccountMasked = MaskAccountNumber(account.NumeroCuenta),
                HolderFullName = holderFullName,
                Amount = request.Amount,
                CurrentBalance = account.Balance
            };

            return Result<WithdrawalPreviewDTO>.Ok(preview);
        }

        public async Task<Result<WithdrawalResultDTO>> ExecuteWithdrawalAsync(WithdrawalDTO command)
        {
            if (command.Amount <= 0)
                return Result<WithdrawalResultDTO>.Fail("El monto a retirar debe ser mayor que cero.");

            var now = DateTime.Now;

            var account = await _savingsAccountRepository
                .GetByAccountNumberAsync(command.AccountNumber);

            if (account is null)
                return Result<WithdrawalResultDTO>.Fail("La cuenta origen no existe.");

            if (!account.IsActive)
                return Result<WithdrawalResultDTO>.Fail("La cuenta origen está inactiva.");

            if (account.Balance < command.Amount)
                return Result<WithdrawalResultDTO>.Fail("Fondos insuficientes en la cuenta.");

            account.Balance -= command.Amount;

            var transaction = new Transaction
            {
                SavingsAccountId = account.Id,
                Monto = command.Amount,
                FechaTransaccion = now,
                Tipo = "DÉBITO",
                Origen = account.NumeroCuenta,
                Beneficiario = "RETIRO",
                Estado = "APROBADA",
                OperationType = TransactionOperationType.Retiro,
                OperatedByUserId = command.OperatedByUserId
            };

            await _transactionRepository.AddAsync(transaction);
            await _savingsAccountRepository.UpdateAsync(account);

            var user = await _userInfoService.GetUserBasicInfoByIdAsync(account.UserId);
            var email = user?.Email ?? string.Empty;

            var result = new WithdrawalResultDTO
            {
                AccountNumber = account.NumeroCuenta,
                Amount = command.Amount,
                NewBalance = account.Balance,
                ExecutedAt = now,
                HolderEmail = email
            };

            var subject = $"Retiro realizado de la cuenta {MaskAccountNumber(account.NumeroCuenta)}";
            var body = $"""
                Hola,

                Se ha realizado un retiro desde su cuenta de ahorro.

                Monto: {command.Amount:C}
                Cuenta: {account.NumeroCuenta}
                Fecha y hora: {now:dd/MM/yyyy HH:mm}

                Si usted no reconoce esta operación, contacte al banco de inmediato.

                ArtemisBanking
                """;

            var emailRequest = new EmailRequestDto
            {
                To = email,
                Subject = subject,
                Body = body
            };

            await _emailService.SendAsync(emailRequest);

            return Result<WithdrawalResultDTO>.Ok(result);
        }
    }
}

