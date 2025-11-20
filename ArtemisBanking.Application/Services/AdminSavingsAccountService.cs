using System.Security.Cryptography;
using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Dtos.Identity;
using ArtemisBanking.Application.Dtos.SavingsAccount;
using ArtemisBanking.Application.Dtos.Transaction;
using ArtemisBanking.Application.Interfaces.Identity;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Services
{
    public class AdminSavingsAccountService : IAdminSavingsAccountService
    {
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IIdentityUserManager _identityUserManager;
        private readonly IMapper _mapper;

        public AdminSavingsAccountService(
            ISavingsAccountRepository savingsAccountRepository,
            ILoanRepository loanRepository,
            ICreditCardRepository creditCardRepository,
            ITransactionRepository transactionRepository,
            IIdentityUserManager identityUserManager,
            IMapper mapper)
        {
            _savingsAccountRepository = savingsAccountRepository;
            _loanRepository = loanRepository;
            _creditCardRepository = creditCardRepository;
            _transactionRepository = transactionRepository;
            _identityUserManager = identityUserManager;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<SavingsAccountListItemDTO>> GetSavingsAccountsAsync(
            int pageNumber,
            int pageSize,
            string? estadoFilter = null,
            string? tipoFilter = null,
            string? cedulaFilter = null,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 20;

            var normalizedEstado = (estadoFilter ?? "ACTIVAS").Trim().ToUpperInvariant();
            var normalizedTipo = (tipoFilter ?? "TODAS").Trim().ToUpperInvariant();

            List<SavingsAccount> cuentas;

            if (!string.IsNullOrWhiteSpace(cedulaFilter))
            {
                var normalizedCedula = NormalizeCedula(cedulaFilter);
                if (string.IsNullOrWhiteSpace(normalizedCedula))
                {
                    return new PaginatedResult<SavingsAccountListItemDTO>(Array.Empty<SavingsAccountListItemDTO>(), pageNumber, pageSize, 0);
                }

                var usuarios = await _identityUserManager.GetAllAsync(cancellationToken);
                var usuario = usuarios.FirstOrDefault(u => NormalizeCedula(u.Cedula) == normalizedCedula);

                if (usuario == null)
                {
                    return new PaginatedResult<SavingsAccountListItemDTO>(Array.Empty<SavingsAccountListItemDTO>(), pageNumber, pageSize, 0);
                }

                cuentas = await _savingsAccountRepository.GetByUserIdIncludingInactiveAsync(usuario.Id);
                cuentas = cuentas
                    .OrderByDescending(c => c.IsActive)
                    .ThenByDescending(c => c.FechaCreacion)
                    .ToList();
            }
            else
            {
                cuentas = await _savingsAccountRepository.GetAllAsync();
            }

            cuentas = normalizedEstado switch
            {
                "CANCELADAS" => cuentas.Where(c => !c.IsActive).ToList(),
                "TODAS" => cuentas,
                _ => cuentas.Where(c => c.IsActive).ToList()
            };

            cuentas = normalizedTipo switch
            {
                "PRINCIPAL" => cuentas.Where(c => c.EsPrincipal).ToList(),
                "SECUNDARIA" => cuentas.Where(c => !c.EsPrincipal).ToList(),
                _ => cuentas
            };

            var totalCount = cuentas.Count;
            var pagedAccounts = cuentas
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var cache = new Dictionary<string, IdentityUserDto?>();
            var dtoList = new List<SavingsAccountListItemDTO>(pagedAccounts.Count);

            foreach (var cuenta in pagedAccounts)
            {
                var dto = _mapper.Map<SavingsAccountListItemDTO>(cuenta);
                var usuario = await GetUserCachedAsync(cuenta.UserId, cache, cancellationToken);

                if (usuario != null)
                {
                    dto.ClienteNombre = usuario.Nombre;
                    dto.ClienteApellido = usuario.Apellido;
                    dto.ClienteCedula = FormatCedula(usuario.Cedula);
                }

                dtoList.Add(dto);
            }

            return new PaginatedResult<SavingsAccountListItemDTO>(dtoList, pageNumber, pageSize, totalCount);
        }

        public async Task<List<ClientForSavingsAccountDTO>> GetEligibleClientsAsync(CancellationToken cancellationToken = default)
        {
            var usuarios = await _identityUserManager.GetAllAsync(cancellationToken);
            var clientes = usuarios
                .Where(u => u.Roles.Contains("Cliente") && u.IsActive)
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.Apellido)
                .ToList();

            var resultado = new List<ClientForSavingsAccountDTO>();

            foreach (var cliente in clientes)
            {
                var prestamos = await _loanRepository.GetByUserIdAsync(cliente.Id);
                var deudaPrestamos = prestamos.Where(p => p.IsActive).Sum(p => p.MontoPendiente);

                var tarjetas = await _creditCardRepository.GetActiveByUserIdAsync(cliente.Id);
                var deudaTarjetas = tarjetas.Sum(t => t.DeudaActual);

                resultado.Add(new ClientForSavingsAccountDTO
                {
                    UserId = cliente.Id,
                    Cedula = FormatCedula(cliente.Cedula),
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Email = cliente.Email,
                    DeudaTotal = deudaPrestamos + deudaTarjetas
                });
            }

            return resultado
                .OrderBy(r => r.Nombre)
                .ThenBy(r => r.Apellido)
                .ToList();
        }

        public async Task<Result> AssignSecondaryAccountAsync(AssignSavingsAccountDTO request, string adminUserId, CancellationToken cancellationToken = default)
        {
            if (request.BalanceInicial < 0)
            {
                return Result.Fail("El balance inicial no puede ser negativo");
            }

            var usuario = await _identityUserManager.GetByIdAsync(request.UserId, cancellationToken);
            if (usuario == null || !usuario.IsActive)
            {
                return Result.Fail("Cliente no encontrado o inactivo");
            }

            var cuentaPrincipal = await _savingsAccountRepository.GetPrincipalByUserIdAsync(request.UserId);
            if (cuentaPrincipal == null)
            {
                // Si no existe cuenta principal, la crea automaticamente (solo por ahora hasta el merge)
                var numeroCuentaPrincipal = await GenerateUniqueAccountNumberAsync(cancellationToken);
                cuentaPrincipal = new SavingsAccount
                {
                    NumeroCuenta = numeroCuentaPrincipal,
                    Balance = 0,
                    EsPrincipal = true,
                    IsActive = true,
                    FechaCreacion = DateTime.UtcNow,
                    UserId = request.UserId
                };
                await _savingsAccountRepository.AddAsync(cuentaPrincipal, cancellationToken);
            }

            var numeroCuenta = await GenerateUniqueAccountNumberAsync(cancellationToken);

            var nuevaCuenta = new SavingsAccount
            {
                NumeroCuenta = numeroCuenta,
                Balance = request.BalanceInicial,
                EsPrincipal = false,
                IsActive = true,
                FechaCreacion = DateTime.UtcNow,
                UserId = request.UserId
            };

            await _savingsAccountRepository.AddAsync(nuevaCuenta, cancellationToken);
            return Result.Ok();
        }

        public async Task<SavingsAccountDetailDTO?> GetAccountDetailAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var cuenta = await _savingsAccountRepository.GetByIdAsync(accountId);
            if (cuenta == null)
            {
                return null;
            }

            var usuario = await _identityUserManager.GetByIdAsync(cuenta.UserId, cancellationToken);
            var transacciones = await _transactionRepository.GetByAccountNumberAsync(cuenta.NumeroCuenta);

            cuenta.Transactions = transacciones;
            var detalle = _mapper.Map<SavingsAccountDetailDTO>(cuenta);
            detalle.Transacciones = _mapper.Map<List<TransactionDTO>>(transacciones);

            detalle.ClienteNombre = usuario?.Nombre ?? string.Empty;
            detalle.ClienteApellido = usuario?.Apellido ?? string.Empty;
            detalle.ClienteCedula = usuario != null
                ? FormatCedula(usuario.Cedula)
                : string.Empty;

            return detalle;
        }

        public async Task<Result> CancelSecondaryAccountAsync(int accountId, string adminUserId, CancellationToken cancellationToken = default)
        {
            var cuenta = await _savingsAccountRepository.GetByIdAsync(accountId);
            if (cuenta == null)
            {
                return Result.Fail("Cuenta no encontrada");
            }

            if (cuenta.EsPrincipal)
            {
                return Result.Fail("Las cuentas principales no pueden cancelarse");
            }

            if (!cuenta.IsActive)
            {
                return Result.Fail("La cuenta ya se encuentra cancelada");
            }

            var cuentaPrincipal = await _savingsAccountRepository.GetPrincipalByUserIdAsync(cuenta.UserId);
            if (cuentaPrincipal == null)
            {
                return Result.Fail("No se encontró una cuenta principal para transferir los fondos");
            }

            if (cuenta.Balance > 0)
            {
                var monto = cuenta.Balance;
                cuentaPrincipal.Balance += monto;
                cuenta.Balance = 0;

                await _savingsAccountRepository.UpdateAsync(cuentaPrincipal);

                var transaccionDebito = new Transaction
                {
                    SavingsAccountId = cuenta.Id,
                    Monto = monto,
                    FechaTransaccion = DateTime.UtcNow,
                    Tipo = "DEBITO",
                    Origen = cuenta.NumeroCuenta,
                    Beneficiario = cuentaPrincipal.NumeroCuenta,
                    Estado = "APROBADA"
                };

                var transaccionCredito = new Transaction
                {
                    SavingsAccountId = cuentaPrincipal.Id,
                    Monto = monto,
                    FechaTransaccion = DateTime.UtcNow,
                    Tipo = "CREDITO",
                    Origen = cuenta.NumeroCuenta,
                    Beneficiario = cuentaPrincipal.NumeroCuenta,
                    Estado = "APROBADA"
                };

                await _transactionRepository.AddAsync(transaccionDebito);
                await _transactionRepository.AddAsync(transaccionCredito);
            }

            cuenta.IsActive = false;
            await _savingsAccountRepository.UpdateAsync(cuenta);

            return Result.Ok();
        }

        private async Task<string> GenerateUniqueAccountNumberAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                var number = RandomNumberGenerator.GetInt32(100_000_000, 1_000_000_000).ToString();
                var existsInAccounts = await _savingsAccountRepository.ExistsByAccountNumberAsync(number);
                var existsInLoans = await _loanRepository.GetByLoanNumberAsync(number) != null;

                if (!existsInAccounts && !existsInLoans)
                {
                    return number;
                }
            }
        }

        private static string NormalizeCedula(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return new string(value.Where(char.IsDigit).ToArray());
        }

        private static string FormatCedula(string? value)
        {
            var digits = NormalizeCedula(value);
            if (digits.Length != 11)
            {
                return digits;
            }

            return $"{digits.Substring(0, 3)}-{digits.Substring(3, 7)}-{digits.Substring(10)}";
        }

        private async Task<IdentityUserDto?> GetUserCachedAsync(
            string userId,
            IDictionary<string, IdentityUserDto?> cache,
            CancellationToken cancellationToken)
        {
            if (cache.TryGetValue(userId, out var usuario))
            {
                return usuario;
            }

            usuario = await _identityUserManager.GetByIdAsync(userId, cancellationToken);
            cache[userId] = usuario;
            return usuario;
        }
    }
}

