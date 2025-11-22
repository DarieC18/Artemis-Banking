using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infrastructure.Persistence;
using ArtemisBanking.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArtemisBanking.Integration.Tests.Persistence.Repositories
{
    public class SavingsAccountRepositoryTests : IDisposable
    {
        private readonly ArtemisBankingDbContext _context;
        private readonly SavingsAccountRepository _repository;

        public SavingsAccountRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ArtemisBankingDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_SavingsAccountRepository_{Guid.NewGuid()}")
                .Options;

            _context = new ArtemisBankingDbContext(options);
            _repository = new SavingsAccountRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ConCuentaValida_GuardaCorrectamente()
        {
            // Arrange
            var cuenta = new SavingsAccount
            {
                NumeroCuenta = "123456789",
                Balance = 10000m,
                EsPrincipal = true,
                IsActive = true,
                FechaCreacion = DateTime.UtcNow,
                UserId = "user-123"
            };

            // Act
            await _repository.AddAsync(cuenta);

            // Assert
            var cuentaGuardada = await _context.SavingsAccounts.FindAsync(cuenta.Id);
            cuentaGuardada.Should().NotBeNull();
            cuentaGuardada!.NumeroCuenta.Should().Be("123456789");
            cuentaGuardada.Balance.Should().Be(10000m);
            cuentaGuardada.EsPrincipal.Should().BeTrue();
            cuentaGuardada.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetByUserIdAsync_ConUsuarioValido_RetornaSoloCuentasActivas()
        {
            // Arrange
            var userId = "user-123";
            var cuentaActiva = new SavingsAccount
            {
                NumeroCuenta = "111111111",
                Balance = 5000m,
                IsActive = true,
                UserId = userId,
                FechaCreacion = DateTime.UtcNow
            };
            var cuentaInactiva = new SavingsAccount
            {
                NumeroCuenta = "222222222",
                Balance = 3000m,
                IsActive = false,
                UserId = userId,
                FechaCreacion = DateTime.UtcNow
            };

            _context.SavingsAccounts.AddRange(cuentaActiva, cuentaInactiva);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetByUserIdAsync(userId);

            // Assert
            resultado.Should().HaveCount(1);
            resultado.First().NumeroCuenta.Should().Be("111111111");
            resultado.First().IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetPrincipalByUserIdAsync_ConUsuarioValido_RetornaCuentaPrincipal()
        {
            // Arrange
            var userId = "user-123";
            var cuentaPrincipal = new SavingsAccount
            {
                NumeroCuenta = "111111111",
                Balance = 5000m,
                EsPrincipal = true,
                IsActive = true,
                UserId = userId,
                FechaCreacion = DateTime.UtcNow
            };
            var cuentaSecundaria = new SavingsAccount
            {
                NumeroCuenta = "222222222",
                Balance = 3000m,
                EsPrincipal = false,
                IsActive = true,
                UserId = userId,
                FechaCreacion = DateTime.UtcNow
            };

            _context.SavingsAccounts.AddRange(cuentaPrincipal, cuentaSecundaria);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetPrincipalByUserIdAsync(userId);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.EsPrincipal.Should().BeTrue();
            resultado.NumeroCuenta.Should().Be("111111111");
        }

        [Fact]
        public async Task GetByAccountNumberAsync_ConNumeroValido_RetornaCuenta()
        {
            // Arrange
            var numeroCuenta = "123456789";
            var cuenta = new SavingsAccount
            {
                NumeroCuenta = numeroCuenta,
                Balance = 10000m,
                IsActive = true,
                UserId = "user-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.SavingsAccounts.Add(cuenta);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetByAccountNumberAsync(numeroCuenta);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.NumeroCuenta.Should().Be(numeroCuenta);
        }

        [Fact]
        public async Task GetByAccountNumberAsync_ConCuentaInactiva_RetornaNull()
        {
            // Arrange
            var numeroCuenta = "123456789";
            var cuenta = new SavingsAccount
            {
                NumeroCuenta = numeroCuenta,
                Balance = 10000m,
                IsActive = false,
                UserId = "user-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.SavingsAccounts.Add(cuenta);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetByAccountNumberAsync(numeroCuenta);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ExistsByAccountNumberAsync_ConNumeroExistente_RetornaTrue()
        {
            // Arrange
            var numeroCuenta = "123456789";
            var cuenta = new SavingsAccount
            {
                NumeroCuenta = numeroCuenta,
                Balance = 10000m,
                IsActive = true,
                UserId = "user-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.SavingsAccounts.Add(cuenta);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.ExistsByAccountNumberAsync(numeroCuenta);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByAccountNumberAsync_ConNumeroInexistente_RetornaFalse()
        {
            // Act
            var resultado = await _repository.ExistsByAccountNumberAsync("999999999");

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ConCuentaExistente_ActualizaCorrectamente()
        {
            // Arrange
            var cuenta = new SavingsAccount
            {
                NumeroCuenta = "123456789",
                Balance = 10000m,
                IsActive = true,
                UserId = "user-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.SavingsAccounts.Add(cuenta);
            await _context.SaveChangesAsync();

            // Act
            cuenta.Balance = 15000m;
            await _repository.UpdateAsync(cuenta);

            // Assert
            var cuentaActualizada = await _context.SavingsAccounts.FindAsync(cuenta.Id);
            cuentaActualizada!.Balance.Should().Be(15000m);
        }

        [Fact]
        public async Task GetAllAsync_RetornaTodasLasCuentasOrdenadasPorFecha()
        {
            // Arrange
            var cuenta1 = new SavingsAccount
            {
                NumeroCuenta = "111111111",
                Balance = 1000m,
                IsActive = true,
                UserId = "user-1",
                FechaCreacion = DateTime.UtcNow.AddDays(-2)
            };
            var cuenta2 = new SavingsAccount
            {
                NumeroCuenta = "222222222",
                Balance = 2000m,
                IsActive = true,
                UserId = "user-2",
                FechaCreacion = DateTime.UtcNow
            };

            _context.SavingsAccounts.AddRange(cuenta1, cuenta2);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetAllAsync();

            // Assert
            resultado.Should().HaveCount(2);
            resultado.First().NumeroCuenta.Should().Be("222222222"); // Más reciente primero
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}

