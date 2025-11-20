using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infrastructure.Persistence;
using ArtemisBanking.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArtemisBanking.Integration.Tests.Persistence.Repositories
{
    public class CreditCardRepositoryTests : IDisposable
    {
        private readonly ArtemisBankingDbContext _context;
        private readonly CreditCardRepository _repository;

        public CreditCardRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ArtemisBankingDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_CreditCardRepository_{Guid.NewGuid()}")
                .Options;

            _context = new ArtemisBankingDbContext(options);
            _repository = new CreditCardRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ConTarjetaValida_GuardaCorrectamente()
        {
            // Arrange
            var tarjeta = new CreditCard
            {
                NumeroTarjeta = "1234567890123456",
                LimiteCredito = 50000m,
                DeudaActual = 0m,
                FechaExpiracion = "12/27",
                CVCHash = "abc123",
                IsActive = true,
                UserId = "user-123",
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };

            // Act
            var resultado = await _repository.AddAsync(tarjeta);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().BeGreaterThan(0);
            resultado.NumeroTarjeta.Should().Be("1234567890123456");
            
            var tarjetaGuardada = await _context.CreditCards.FindAsync(tarjeta.Id);
            tarjetaGuardada.Should().NotBeNull();
        }

        [Fact]
        public async Task GetActiveByUserIdAsync_ConUsuarioValido_RetornaSoloTarjetasActivas()
        {
            // Arrange
            var userId = "user-123";
            var tarjetaActiva = new CreditCard
            {
                NumeroTarjeta = "1111111111111111",
                LimiteCredito = 50000m,
                FechaExpiracion = "12/27",
                CVCHash = "hash1",
                IsActive = true,
                UserId = userId,
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };
            var tarjetaCancelada = new CreditCard
            {
                NumeroTarjeta = "2222222222222222",
                LimiteCredito = 30000m,
                FechaExpiracion = "12/27",
                CVCHash = "hash2",
                IsActive = false,
                UserId = userId,
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.CreditCards.AddRange(tarjetaActiva, tarjetaCancelada);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetActiveByUserIdAsync(userId);

            // Assert
            resultado.Should().HaveCount(1);
            resultado.First().IsActive.Should().BeTrue();
            resultado.First().NumeroTarjeta.Should().Be("1111111111111111");
        }

        [Fact]
        public async Task GetByIdAsync_ConTarjetaConConsumos_RetornaConConsumos()
        {
            // Arrange
            var tarjeta = new CreditCard
            {
                NumeroTarjeta = "1234567890123456",
                LimiteCredito = 50000m,
                FechaExpiracion = "12/27",
                CVCHash = "hash123",
                IsActive = true,
                UserId = "user-123",
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };
            _context.CreditCards.Add(tarjeta);
            await _context.SaveChangesAsync();

            var consumo = new CreditCardConsumption
            {
                CreditCardId = tarjeta.Id,
                Monto = 5000m,
                Comercio = "Supermercado",
                FechaConsumo = DateTime.UtcNow,
                Estado = "APROBADO"
            };
            _context.CreditCardConsumptions.Add(consumo);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetByIdAsync(tarjeta.Id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Consumos.Should().HaveCount(1);
            resultado.Consumos.First().Monto.Should().Be(5000m);
        }

        [Fact]
        public async Task GetByNumberAsync_ConNumeroValido_RetornaTarjeta()
        {
            // Arrange
            var numeroTarjeta = "1234567890123456";
            var tarjeta = new CreditCard
            {
                NumeroTarjeta = numeroTarjeta,
                LimiteCredito = 50000m,
                FechaExpiracion = "12/27",
                CVCHash = "hash123",
                IsActive = true,
                UserId = "user-123",
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.CreditCards.Add(tarjeta);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetByNumberAsync(numeroTarjeta);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.NumeroTarjeta.Should().Be(numeroTarjeta);
        }

        [Fact]
        public async Task GetByNumberAsync_ConTarjetaInactiva_RetornaNull()
        {
            // Arrange
            var numeroTarjeta = "1234567890123456";
            var tarjeta = new CreditCard
            {
                NumeroTarjeta = numeroTarjeta,
                LimiteCredito = 50000m,
                FechaExpiracion = "12/27",
                CVCHash = "hash123",
                IsActive = false,
                UserId = "user-123",
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.CreditCards.Add(tarjeta);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetByNumberAsync(numeroTarjeta);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ExistsByNumberAsync_ConNumeroExistente_RetornaTrue()
        {
            // Arrange
            var numeroTarjeta = "1234567890123456";
            var tarjeta = new CreditCard
            {
                NumeroTarjeta = numeroTarjeta,
                LimiteCredito = 50000m,
                FechaExpiracion = "12/27",
                CVCHash = "hash123",
                IsActive = true,
                UserId = "user-123",
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.CreditCards.Add(tarjeta);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.ExistsByNumberAsync(numeroTarjeta);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByNumberAsync_ConNumeroInexistente_RetornaFalse()
        {
            // Act
            var resultado = await _repository.ExistsByNumberAsync("9999999999999999");

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ConTarjetaExistente_ActualizaCorrectamente()
        {
            // Arrange
            var tarjeta = new CreditCard
            {
                NumeroTarjeta = "1234567890123456",
                LimiteCredito = 50000m,
                DeudaActual = 10000m,
                FechaExpiracion = "12/27",
                CVCHash = "hash123",
                IsActive = true,
                UserId = "user-123",
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.CreditCards.Add(tarjeta);
            await _context.SaveChangesAsync();

            // Act
            tarjeta.LimiteCredito = 75000m;
            tarjeta.DeudaActual = 15000m;
            await _repository.UpdateAsync(tarjeta);

            // Assert
            var tarjetaActualizada = await _context.CreditCards.FindAsync(tarjeta.Id);
            tarjetaActualizada!.LimiteCredito.Should().Be(75000m);
            tarjetaActualizada.DeudaActual.Should().Be(15000m);
        }

        [Fact]
        public async Task GetAverageDebtAsync_ConTarjetasActivas_CalculaPromedioCorrecto()
        {
            // Arrange
            var user1 = "user-1";
            var user2 = "user-2";
            
            var tarjeta1 = new CreditCard
            {
                NumeroTarjeta = "1111111111111111",
                LimiteCredito = 50000m,
                DeudaActual = 20000m,
                FechaExpiracion = "12/27",
                CVCHash = "hash1",
                IsActive = true,
                UserId = user1,
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };
            var tarjeta2 = new CreditCard
            {
                NumeroTarjeta = "2222222222222222",
                LimiteCredito = 30000m,
                DeudaActual = 10000m,
                FechaExpiracion = "12/27",
                CVCHash = "hash2",
                IsActive = true,
                UserId = user2,
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.CreditCards.AddRange(tarjeta1, tarjeta2);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetAverageDebtAsync();

            // Assert
            resultado.Should().Be(15000m); // (20000 + 10000) / 2 = 15000
        }

        [Fact]
        public async Task GetAverageDebtAsync_SinTarjetasActivas_RetornaCero()
        {
            // Act
            var resultado = await _repository.GetAverageDebtAsync();

            // Assert
            resultado.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_ConFiltroEstadoActiva_RetornaSoloActivas()
        {
            // Arrange
            var tarjetaActiva = new CreditCard
            {
                NumeroTarjeta = "1111111111111111",
                LimiteCredito = 50000m,
                FechaExpiracion = "12/27",
                CVCHash = "hash1",
                IsActive = true,
                UserId = "user-1",
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };
            var tarjetaCancelada = new CreditCard
            {
                NumeroTarjeta = "2222222222222222",
                LimiteCredito = 30000m,
                FechaExpiracion = "12/27",
                CVCHash = "hash2",
                IsActive = false,
                UserId = "user-2",
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.CreditCards.AddRange(tarjetaActiva, tarjetaCancelada);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetAllAsync("ACTIVA");

            // Assert
            resultado.Should().HaveCount(1);
            resultado.First().IsActive.Should().BeTrue();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}

