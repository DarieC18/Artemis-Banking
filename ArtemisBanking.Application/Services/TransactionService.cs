using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;

        public TransactionService(
            ISavingsAccountRepository savingsAccountRepository,
            ITransactionRepository transactionRepository,
            IMapper mapper)
        {
            _savingsAccountRepository = savingsAccountRepository;
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        public async Task ExecuteTransactionExpressAsync(string userId, TransactionExpressViewModel model)
        {
            if (model.Monto <= 0)
                throw new ArgumentException("El monto debe ser mayor que cero.");

            if (string.IsNullOrWhiteSpace(model.CuentaOrigen))
                throw new ArgumentException("La cuenta de origen es obligatoria.");

            if (string.IsNullOrWhiteSpace(model.CuentaDestino))
                throw new ArgumentException("La cuenta destino es obligatoria.");

            var cuentaOrigen = await _savingsAccountRepository.GetByAccountNumberAsync(model.CuentaOrigen);
            var cuentaDestino = await _savingsAccountRepository.GetByAccountNumberAsync(model.CuentaDestino);

            if (cuentaOrigen is null)
                throw new InvalidOperationException("La cuenta de origen no existe.");

            if (cuentaOrigen.UserId != userId)
                throw new InvalidOperationException("La cuenta de origen no pertenece al usuario autenticado.");

            if (cuentaDestino is null || !cuentaDestino.IsActive)
                throw new InvalidOperationException("La cuenta destino no existe o está inactiva.");

            if (cuentaOrigen.Balance < model.Monto)
                throw new InvalidOperationException("Fondos insuficientes en la cuenta de origen.");

            cuentaOrigen.Balance -= model.Monto;
            cuentaDestino.Balance += model.Monto;

            await _savingsAccountRepository.UpdateAsync(cuentaOrigen);
            await _savingsAccountRepository.UpdateAsync(cuentaDestino);

            var ahora = DateTime.UtcNow;

            var transOrigen = new Transaction
            {
                SavingsAccountId = cuentaOrigen.Id,
                Monto = model.Monto,
                FechaTransaccion = ahora,
                Tipo = "DEBITO",
                Beneficiario = cuentaDestino.NumeroCuenta,
                Origen = cuentaOrigen.NumeroCuenta,
                Estado = "APROBADA"
            };

            var transDestino = new Transaction
            {
                SavingsAccountId = cuentaDestino.Id,
                Monto = model.Monto,
                FechaTransaccion = ahora,
                Tipo = "CREDITO",
                Beneficiario = cuentaDestino.NumeroCuenta,
                Origen = cuentaOrigen.NumeroCuenta,
                Estado = "APROBADA"
            };

            await _transactionRepository.AddAsync(transOrigen);
            await _transactionRepository.AddAsync(transDestino);
        }

        public Task ExecuteTransactionBeneficiaryAsync(string userId, TransactionBeneficiaryViewModel model)
            => throw new NotImplementedException();

        public Task ExecutePayCreditCardAsync(string userId, PayCreditCardViewModel model)
            => throw new NotImplementedException();

        public Task ExecutePayLoanAsync(string userId, PayLoanViewModel model)
            => throw new NotImplementedException();

        public Task ExecuteCashAdvanceAsync(string userId, CashAdvanceViewModel model)
            => throw new NotImplementedException();

        public Task ExecuteTransferBetweenAccountsAsync(string userId, TransferBetweenAccountsViewModel model)
            => throw new NotImplementedException();
    }
}
