using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs.Common;
using ArtemisBanking.Application.DTOs.Email;
using ArtemisBanking.Application.DTOs.Hermes;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Interfaces.Persistence;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Entities;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ArtemisBanking.Application.Services
{
    public class HermesPayService : IHermesPayService
    {
        private readonly IGenericRepository<Commerce> _commerceRepo;
        private readonly IUserInfoService _userInfo;
        private readonly ISavingsAccountRepository _savingsRepo;
        private readonly ICreditCardRepository _cardRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly ICreditCardConsumptionRepository _consumptionRepo;
        private readonly IEmailService _emailService;

        public HermesPayService(
            IGenericRepository<Commerce> commerceRepo,
            IUserInfoService userInfo,
            ISavingsAccountRepository savingsRepo,
            ICreditCardRepository cardRepo,
            ITransactionRepository transactionRepo,
            ICreditCardConsumptionRepository consumptionRepo,
            IEmailService emailService)
        {
            _commerceRepo = commerceRepo;
            _userInfo = userInfo;
            _savingsRepo = savingsRepo;
            _cardRepo = cardRepo;
            _transactionRepo = transactionRepo;
            _consumptionRepo = consumptionRepo;
            _emailService = emailService;
        }

        public async Task<Result<PagedResult<CommerceTransactionDto>>> GetTransactionsAsync(
            int commerceId, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var commerce = await _commerceRepo.GetById(commerceId);
            if (commerce is null)
                return Result<PagedResult<CommerceTransactionDto>>.Fail("El comercio no existe.");

            var commerceUser = await _userInfo.GetUserBasicInfoByCommerceIdAsync(commerceId);
            if (commerceUser is null)
                return Result<PagedResult<CommerceTransactionDto>>.Fail("El comercio no tiene usuario asociado.");

            var principalAccount = await _savingsRepo.GetPrincipalByUserIdAsync(commerceUser.Id);
            if (principalAccount is null)
                return Result<PagedResult<CommerceTransactionDto>>.Fail("El comercio no tiene cuenta principal.");

            var allTransactions = await _transactionRepo.GetByAccountNumberAsync(principalAccount.NumeroCuenta);

            var ordered = allTransactions
                .OrderByDescending(t => t.FechaTransaccion)
                .ToList();

            int total = ordered.Count;
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var pageItems = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new CommerceTransactionDto
                {
                    Fecha = t.FechaTransaccion,
                    Monto = t.Monto,
                    Tipo = t.Tipo,
                    Beneficiario = t.Beneficiario,
                    Origen = t.Origen,
                    Estado = t.Estado
                })
                .ToList();

            var paged = new PagedResult<CommerceTransactionDto>
            {
                Data = pageItems,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = total
            };

            return Result<PagedResult<CommerceTransactionDto>>.Ok(paged);
        }
        public async Task<Result> ProcessPaymentAsync(int commerceId, ProcessPaymentRequestDto request)
        {
            if (request is null)
                return Result.Fail("Datos incompletos.");

            if (string.IsNullOrWhiteSpace(request.CardNumber) ||
                string.IsNullOrWhiteSpace(request.MonthExpirationCard) ||
                string.IsNullOrWhiteSpace(request.YearExpirationCard) ||
                string.IsNullOrWhiteSpace(request.CVC) ||
                request.TransactionAmount <= 0)
            {
                return Result.Fail("Datos inválidos.");
            }

            var commerce = await _commerceRepo.GetById(commerceId);
            if (commerce is null || !commerce.IsActive)
                return Result.Fail("El comercio no existe o está inactivo.");

            var commerceUser = await _userInfo.GetUserBasicInfoByCommerceIdAsync(commerceId);
            if (commerceUser is null)
                return Result.Fail("El comercio no tiene usuario asociado.");

            var principalAccount = await _savingsRepo.GetPrincipalByUserIdAsync(commerceUser.Id);
            if (principalAccount is null || !principalAccount.IsActive)
                return Result.Fail("El comercio no tiene cuenta principal activa.");

            var card = await _cardRepo.GetByNumberAsync(request.CardNumber);
            if (card is null || !card.IsActive)
                return Result.Fail("La tarjeta no es válida o está inactiva.");

            var expFromRequest = $"{request.MonthExpirationCard}/{request.YearExpirationCard[^2..]}";
            if (!string.Equals(card.FechaExpiracion, expFromRequest))
                return Result.Fail("Fecha de expiración incorrecta.");

            if (!DateTime.TryParseExact(card.FechaExpiracion, "MM/yy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var expDate))
                return Result.Fail("Formato de expiración inválido.");

            var lastDay = new DateTime(expDate.Year, expDate.Month, 1)
                .AddMonths(1).AddDays(-1);

            if (lastDay < DateTime.UtcNow.Date)
                return Result.Fail("La tarjeta está vencida.");

            var cvcHash = ComputeSha256(request.CVC);
            if (!string.Equals(card.CVCHash, cvcHash, StringComparison.OrdinalIgnoreCase))
                return Result.Fail("El CVC no es válido.");

            if (card.DeudaActual + request.TransactionAmount > card.LimiteCredito)
                return Result.Fail("El consumo excede el límite disponible.");

            card.DeudaActual += request.TransactionAmount;
            principalAccount.Balance += request.TransactionAmount;

            await _cardRepo.UpdateAsync(card);
            await _savingsRepo.UpdateAsync(principalAccount);

            var consumo = new CreditCardConsumption
            {
                CreditCardId = card.Id,
                Monto = request.TransactionAmount,
                FechaConsumo = DateTime.UtcNow,
                Comercio = commerce.Name,
                Estado = "APROBADO",
                EsAvanceEfectivo = false
            };

            await _consumptionRepo.AddAsync(consumo);

            var trx = new Transaction
            {
                SavingsAccountId = principalAccount.Id,
                FechaTransaccion = DateTime.UtcNow,
                Monto = request.TransactionAmount,
                Tipo = "CRÉDITO",
                Beneficiario = principalAccount.NumeroCuenta,
                Origen = $"TARJETA ****{card.NumeroTarjeta[^4..]}",
                Estado = "APROBADA",
                OperatedByUserId = commerceUser.Id
            };

            await _transactionRepo.AddAsync(trx);

            var cardOwner = await _userInfo.GetUserBasicInfoByIdAsync(card.UserId);
            if (cardOwner is not null)
            {
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = cardOwner.Email,
                    Subject = $"Consumo realizado con tarjeta ****{card.NumeroTarjeta[^4..]}",
                    Body = $"""
                    Se ha realizado un consumo.

                    Monto: {request.TransactionAmount:C}
                    Comercio: {commerce.Name}
                    Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}
                    """
                });
            }

            await _emailService.SendAsync(new EmailRequestDto
            {
                To = commerceUser.Email,
                Subject = $"Pago recibido ****{card.NumeroTarjeta[^4..]}",
                Body = $"""
                Ha recibido un nuevo pago.

                Monto: {request.TransactionAmount:C}
                Tarjeta: ****{card.NumeroTarjeta[^4..]}
                Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}
                """
            });

            return Result.Ok();
        }

        private static string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(
                sha.ComputeHash(Encoding.UTF8.GetBytes(input))
            ).Replace("-", "").ToLowerInvariant();
        }
    }
}
