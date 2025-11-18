using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Application.Dtos.Transaction;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ISavingsAccountRepository _cuentas;
        private readonly IBeneficiaryRepository _beneficiarios;
        private readonly ILoanRepository _prestamos;
        private readonly ILoanPaymentScheduleRepository _cuotas;
        private readonly ICreditCardRepository _tarjetas;
        private readonly ITransactionRepository _transacciones;

        public TransactionService(
            ISavingsAccountRepository cuentas,
            IBeneficiaryRepository beneficiarios,
            ILoanRepository prestamos,
            ILoanPaymentScheduleRepository cuotas,
            ICreditCardRepository tarjetas,
            ITransactionRepository transacciones)
        {
            _cuentas = cuentas;
            _beneficiarios = beneficiarios;
            _prestamos = prestamos;
            _cuotas = cuotas;
            _tarjetas = tarjetas;
            _transacciones = transacciones;
        }
        public async Task CreateTransactionExpressAsync(string userId, CreateTransactionExpressDTO dto)
        {
            var origen = await _cuentas.GetByAccountNumberAsync(dto.CuentaOrigen);
            if (origen == null || origen.UserId != userId)
                throw new Exception("La cuenta de origen no existe o no pertenece al usuario");

            if (origen.Balance < dto.Monto)
                throw new Exception("Fondos insuficientes");

            var destino = await _cuentas.GetByAccountNumberAsync(dto.CuentaDestino);

            origen.Balance -= dto.Monto;
            await _cuentas.UpdateAsync(origen);

            if (destino != null)
            {
                destino.Balance += dto.Monto;
                await _cuentas.UpdateAsync(destino);
            }

            var trans = new Transaction
            {
                SavingsAccountId = origen.Id,
                Monto = dto.Monto,
                FechaTransaccion = DateTime.UtcNow,
                Tipo = "Transferencia",
                Origen = dto.CuentaOrigen,
                Beneficiario = dto.CuentaDestino,
                Estado = "Completada"
            };

            await _transacciones.AddAsync(trans);
        }

        public async Task CreateTransactionToBeneficiaryAsync(string userId, CreateTransactionBeneficiaryDTO dto)
        {
            var origen = await _cuentas.GetByAccountNumberAsync(dto.CuentaOrigen);
            if (origen == null || origen.UserId != userId)
                throw new Exception("La cuenta de origen no existe o no pertenece al usuario");

            if (origen.Balance < dto.Monto)
                throw new Exception("Fondos insuficientes");

            var beneficiario = await _beneficiarios.GetByIdAsync(dto.BeneficiaryId);
            if (beneficiario == null)
                throw new Exception("Beneficiario no encontrado");

            origen.Balance -= dto.Monto;
            await _cuentas.UpdateAsync(origen);

            var trans = new Transaction
            {
                SavingsAccountId = origen.Id,
                Monto = dto.Monto,
                FechaTransaccion = DateTime.UtcNow,
                Tipo = "Transferencia a beneficiario",
                Origen = dto.CuentaOrigen,
                Beneficiario = $"BeneficiarioId:{beneficiario.Id}",
                Estado = "Completada"
            };

            await _transacciones.AddAsync(trans);
        }

        public async Task TransferBetweenAccountsAsync(string userId, TransferBetweenAccountsDTO dto)
        {
            var origen = await _cuentas.GetByAccountNumberAsync(dto.CuentaOrigen);
            if (origen == null || origen.UserId != userId)
                throw new Exception("La cuenta de origen no existe o no pertenece al usuario");

            var destino = await _cuentas.GetByAccountNumberAsync(dto.CuentaDestino);
            if (destino == null || destino.UserId != userId)
                throw new Exception("La cuenta de destino no existe o no pertenece al usuario");

            if (origen.Id == destino.Id)
                throw new Exception("La cuenta de origen y destino no pueden ser la misma");

            if (origen.Balance < dto.Monto)
                throw new Exception("Fondos insuficientes");

            origen.Balance -= dto.Monto;
            destino.Balance += dto.Monto;

            await _cuentas.UpdateAsync(origen);
            await _cuentas.UpdateAsync(destino);

            var trans = new Transaction
            {
                SavingsAccountId = origen.Id,
                Monto = dto.Monto,
                FechaTransaccion = DateTime.UtcNow,
                Tipo = "Transferencia entre cuentas",
                Origen = dto.CuentaOrigen,
                Beneficiario = dto.CuentaDestino,
                Estado = "Completada"
            };

            await _transacciones.AddAsync(trans);
        }

        public async Task PayLoanAsync(string userId, PayLoanDTO dto)
        {
            var origen = await _cuentas.GetByAccountNumberAsync(dto.CuentaOrigen);
            if (origen == null || origen.UserId != userId)
                throw new Exception("La cuenta de origen no existe o no pertenece al usuario");

            if (origen.Balance < dto.Monto)
                throw new Exception("Fondos insuficientes");

            var prestamo = await _prestamos.GetByIdWithScheduleAsync(dto.LoanId);
            if (prestamo == null)
                throw new Exception("Préstamo no encontrado");

            origen.Balance -= dto.Monto;
            await _cuentas.UpdateAsync(origen);

            var trans = new Transaction
            {
                SavingsAccountId = origen.Id,
                Monto = dto.Monto,
                FechaTransaccion = DateTime.UtcNow,
                Tipo = "Pago de préstamo",
                Origen = dto.CuentaOrigen,
                Beneficiario = $"LoanId:{dto.LoanId}",
                Estado = "Completada"
            };

            await _transacciones.AddAsync(trans);
        }

        public async Task PayCreditCardAsync(string userId, PayCreditCardDTO dto)
        {
            var origen = await _cuentas.GetByAccountNumberAsync(dto.CuentaOrigen);
            if (origen == null || origen.UserId != userId)
                throw new Exception("La cuenta de origen no existe o no pertenece al usuario");

            if (origen.Balance < dto.Monto)
                throw new Exception("Fondos insuficientes");

            var tarjeta = await _tarjetas.GetByIdAsync(dto.CreditCardId);
            if (tarjeta == null || tarjeta.UserId != userId)
                throw new Exception("Tarjeta no encontrada o no pertenece al usuario");

            if (!tarjeta.IsActive)
                throw new Exception("La tarjeta no está activa");

            origen.Balance -= dto.Monto;
            await _cuentas.UpdateAsync(origen);

            // Resta la6 deuda de tarjeta
            tarjeta.DeudaActual -= dto.Monto;
            if (tarjeta.DeudaActual < 0)
                tarjeta.DeudaActual = 0;

            await _tarjetas.UpdateAsync(tarjeta);

            var trans = new Transaction
            {
                SavingsAccountId = origen.Id,
                Monto = dto.Monto,
                FechaTransaccion = DateTime.UtcNow,
                Tipo = "Pago de tarjeta",
                Origen = dto.CuentaOrigen,
                Beneficiario = $"CreditCard:{tarjeta.NumeroTarjeta}",
                Estado = "Completada"
            };

            await _transacciones.AddAsync(trans);
        }

        public async Task CreateCashAdvanceAsync(string userId, CashAdvanceDTO dto)
        {
            var destino = await _cuentas.GetByAccountNumberAsync(dto.CuentaDestino);
            if (destino == null || destino.UserId != userId)
                throw new Exception("La cuenta de destino no existe o no pertenece al usuario");

            var tarjeta = await _tarjetas.GetByIdAsync(dto.CreditCardId);
            if (tarjeta == null || tarjeta.UserId != userId)
                throw new Exception("Tarjeta no encontrada o no pertenece al usuario");

            if (!tarjeta.IsActive)
                throw new Exception("La tarjeta no está activa");

            var disponible = tarjeta.LimiteCredito - tarjeta.DeudaActual;
            if (dto.Monto > disponible)
                throw new Exception("Crédito insuficiente para realizar el avance");

            tarjeta.DeudaActual += dto.Monto;
            await _tarjetas.UpdateAsync(tarjeta);

            destino.Balance += dto.Monto;
            await _cuentas.UpdateAsync(destino);

            var trans = new Transaction
            {
                SavingsAccountId = destino.Id,
                Monto = dto.Monto,
                FechaTransaccion = DateTime.UtcNow,
                Tipo = "Avance de efectivo",
                Origen = $"Tarjeta:{tarjeta.NumeroTarjeta}",
                Beneficiario = dto.CuentaDestino,
                Estado = "Completada"
            };

            await _transacciones.AddAsync(trans);
        }
    }
}