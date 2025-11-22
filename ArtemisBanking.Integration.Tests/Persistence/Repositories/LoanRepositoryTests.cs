using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infrastructure.Persistence;
using ArtemisBanking.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArtemisBanking.Integration.Tests.Persistence.Repositories
{
    public class LoanRepositoryTests : IDisposable
    {
        private readonly ArtemisBankingDbContext _context;
        private readonly LoanRepository _repository;

        public LoanRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ArtemisBankingDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_LoanRepository_{Guid.NewGuid()}")
                .Options;

            _context = new ArtemisBankingDbContext(options);
            _repository = new LoanRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ConPrestamoValido_GuardaCorrectamente()
        {
            // Arrange
            var prestamo = new Loan
            {
                NumeroPrestamo = "PREST-001",
                MontoCapital = 50000m,
                CuotasTotales = 12,
                CuotasPagadas = 0,
                MontoPendiente = 54000m,
                TasaInteres = 15.5m,
                PlazoMeses = 12,
                IsActive = true,
                FechaCreacion = DateTime.UtcNow,
                UserId = "user-123",
                AdminUserId = "admin-123"
            };

            // Act
            var resultado = await _repository.AddAsync(prestamo);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().BeGreaterThan(0);
            resultado.NumeroPrestamo.Should().Be("PREST-001");
            
            var prestamoGuardado = await _context.Loans.FindAsync(prestamo.Id);
            prestamoGuardado.Should().NotBeNull();
        }

        [Fact]
        public async Task GetActiveByUserIdAsync_ConUsuarioValido_RetornaSoloPrestamosActivos()
        {
            // Arrange
            var userId = "user-123";
            var prestamoActivo = new Loan
            {
                NumeroPrestamo = "PREST-001",
                MontoCapital = 50000m,
                IsActive = true,
                UserId = userId,
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };
            var prestamoCompletado = new Loan
            {
                NumeroPrestamo = "PREST-002",
                MontoCapital = 30000m,
                IsActive = false,
                UserId = userId,
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.Loans.AddRange(prestamoActivo, prestamoCompletado);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetActiveByUserIdAsync(userId);

            // Assert
            resultado.Should().HaveCount(1);
            resultado.First().IsActive.Should().BeTrue();
            resultado.First().NumeroPrestamo.Should().Be("PREST-001");
        }

        [Fact]
        public async Task GetByIdWithScheduleAsync_ConPrestamoConCuotas_RetornaConTablaAmortizacion()
        {
            // Arrange
            var prestamo = new Loan
            {
                NumeroPrestamo = "PREST-001",
                MontoCapital = 50000m,
                IsActive = true,
                UserId = "user-123",
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };
            _context.Loans.Add(prestamo);
            await _context.SaveChangesAsync();

            var cuota = new LoanPaymentSchedule
            {
                LoanId = prestamo.Id,
                NumeroCuota = 1,
                ValorCuota = 4500m,
                FechaPago = DateTime.UtcNow.AddMonths(1),
                Pagada = false
            };
            _context.LoanPaymentSchedules.Add(cuota);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetByIdWithScheduleAsync(prestamo.Id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.TablaAmortizacion.Should().HaveCount(1);
            resultado.TablaAmortizacion.First().NumeroCuota.Should().Be(1);
        }

        [Fact]
        public async Task HasActiveLoanAsync_ConPrestamoActivo_RetornaTrue()
        {
            // Arrange
            var userId = "user-123";
            var prestamo = new Loan
            {
                NumeroPrestamo = "PREST-001",
                MontoCapital = 50000m,
                IsActive = true,
                UserId = userId,
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.Loans.Add(prestamo);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.HasActiveLoanAsync(userId);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveLoanAsync_SinPrestamoActivo_RetornaFalse()
        {
            // Arrange
            var userId = "user-123";

            // Act
            var resultado = await _repository.HasActiveLoanAsync(userId);

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task GetAverageDebtAsync_ConPrestamosActivos_CalculaPromedioCorrecto()
        {
            // Arrange
            var user1 = "user-1";
            var user2 = "user-2";
            
            var prestamo1 = new Loan
            {
                NumeroPrestamo = "PREST-001",
                MontoCapital = 50000m,
                MontoPendiente = 50000m,
                IsActive = true,
                UserId = user1,
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };
            var prestamo2 = new Loan
            {
                NumeroPrestamo = "PREST-002",
                MontoCapital = 30000m,
                MontoPendiente = 30000m,
                IsActive = true,
                UserId = user2,
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.Loans.AddRange(prestamo1, prestamo2);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetAverageDebtAsync();

            // Assert
            resultado.Should().Be(40000m); // (50000 + 30000) / 2 = 40000
        }

        [Fact]
        public async Task GetAverageDebtAsync_SinPrestamosActivos_RetornaCero()
        {
            // Act
            var resultado = await _repository.GetAverageDebtAsync();

            // Assert
            resultado.Should().Be(0);
        }

        [Fact]
        public async Task GetByLoanNumberAsync_ConNumeroValido_RetornaPrestamo()
        {
            // Arrange
            var numeroPrestamo = "PREST-001";
            var prestamo = new Loan
            {
                NumeroPrestamo = numeroPrestamo,
                MontoCapital = 50000m,
                IsActive = true,
                UserId = "user-123",
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.Loans.Add(prestamo);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetByLoanNumberAsync(numeroPrestamo);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.NumeroPrestamo.Should().Be(numeroPrestamo);
        }

        [Fact]
        public async Task UpdateAsync_ConPrestamoExistente_ActualizaCorrectamente()
        {
            // Arrange
            var prestamo = new Loan
            {
                NumeroPrestamo = "PREST-001",
                MontoCapital = 50000m,
                MontoPendiente = 50000m,
                IsActive = true,
                UserId = "user-123",
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.Loans.Add(prestamo);
            await _context.SaveChangesAsync();

            // Act
            prestamo.MontoPendiente = 45000m;
            prestamo.CuotasPagadas = 1;
            await _repository.UpdateAsync(prestamo);

            // Assert
            var prestamoActualizado = await _context.Loans.FindAsync(prestamo.Id);
            prestamoActualizado!.MontoPendiente.Should().Be(45000m);
            prestamoActualizado.CuotasPagadas.Should().Be(1);
        }

        [Fact]
        public async Task GetAllActiveAsync_RetornaSoloPrestamosActivos()
        {
            // Arrange
            var prestamoActivo = new Loan
            {
                NumeroPrestamo = "PREST-001",
                MontoCapital = 50000m,
                IsActive = true,
                UserId = "user-1",
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };
            var prestamoCompletado = new Loan
            {
                NumeroPrestamo = "PREST-002",
                MontoCapital = 30000m,
                IsActive = false,
                UserId = "user-2",
                AdminUserId = "admin-123",
                FechaCreacion = DateTime.UtcNow
            };

            _context.Loans.AddRange(prestamoActivo, prestamoCompletado);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetAllActiveAsync();

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

