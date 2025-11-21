using ArtemisBanking.Application.Dtos.Transaction;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.Services;
using ArtemisBanking.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ArtemisBanking.Unit.Tests.Services;

public class TransactionServiceTests
{
    private readonly Mock<ISavingsAccountRepository> _savingsAccountRepositoryMock;
    private readonly Mock<IBeneficiaryRepository> _beneficiaryRepositoryMock;
    private readonly Mock<ILoanRepository> _loanRepositoryMock;
    private readonly Mock<ILoanPaymentScheduleRepository> _loanPaymentScheduleRepositoryMock;
    private readonly Mock<ICreditCardRepository> _creditCardRepositoryMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUserInfoService> _userInfoServiceMock;
    private readonly Mock<ICreditCardConsumptionRepository> _creditCardConsumptionRepositoryMock;
    private readonly TransactionService _transactionService;

    public TransactionServiceTests()
    {
        _savingsAccountRepositoryMock = new Mock<ISavingsAccountRepository>();
        _beneficiaryRepositoryMock = new Mock<IBeneficiaryRepository>();
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _loanPaymentScheduleRepositoryMock = new Mock<ILoanPaymentScheduleRepository>();
        _creditCardRepositoryMock = new Mock<ICreditCardRepository>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _userInfoServiceMock = new Mock<IUserInfoService>();
        _creditCardConsumptionRepositoryMock = new Mock<ICreditCardConsumptionRepository>();
        
        _transactionService = new TransactionService(
            _savingsAccountRepositoryMock.Object,
            _beneficiaryRepositoryMock.Object,
            _loanRepositoryMock.Object,
            _loanPaymentScheduleRepositoryMock.Object,
            _creditCardRepositoryMock.Object,
            _transactionRepositoryMock.Object,
            _emailServiceMock.Object,
            _userInfoServiceMock.Object,
            _creditCardConsumptionRepositoryMock.Object
        );
    }

    #region Pruebas de CreateTransactionExpressAsync

    [Fact]
    public async Task CreateTransactionExpressAsync_MontoInvalido_DeberiaLanzarExcepcion()
    {
        // Arrange
        var userId = "user123";
        var dto = new CreateTransactionExpressDTO
        {
            CuentaOrigen = "12345",
            CuentaDestino = "67890",
            Monto = -100,
            UserId = userId
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _transactionService.CreateTransactionExpressAsync(userId, dto));
    }

    [Fact]
    public async Task CreateTransactionExpressAsync_CuentaOrigenNoExiste_DeberiaLanzarExcepcion()
    {
        // Arrange
        var userId = "user123";
        var dto = new CreateTransactionExpressDTO
        {
            CuentaOrigen = "12345",
            CuentaDestino = "67890",
            Monto = 100,
            UserId = userId
        };

        _savingsAccountRepositoryMock
            .Setup(x => x.GetByAccountNumberAsync(dto.CuentaOrigen))
            .ReturnsAsync((SavingsAccount?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _transactionService.CreateTransactionExpressAsync(userId, dto));
    }

    [Fact]
    public async Task CreateTransactionExpressAsync_CuentaDestinoNoExiste_DeberiaCompletarTransaccion()
    {
        // Arrange
        var userId = "user123";
        var dto = new CreateTransactionExpressDTO
        {
            CuentaOrigen = "12345",
            CuentaDestino = "67890",
            Monto = 100,
            UserId = userId
        };

        var cuentaOrigen = new SavingsAccount
        {
            Id = 1,
            NumeroCuenta = "12345",
            Balance = 500,
            IsActive = true,
            UserId = userId
        };

        _savingsAccountRepositoryMock
            .Setup(x => x.GetByAccountNumberAsync(dto.CuentaOrigen))
            .ReturnsAsync(cuentaOrigen);

        _savingsAccountRepositoryMock
            .Setup(x => x.GetByAccountNumberAsync(dto.CuentaDestino))
            .ReturnsAsync((SavingsAccount?)null);

        _savingsAccountRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<SavingsAccount>()))
            .Returns(Task.CompletedTask);

        _transactionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask);

        // Act
        await _transactionService.CreateTransactionExpressAsync(userId, dto);

        // Assert
        cuentaOrigen.Balance.Should().Be(400); // 500 - 100
        _savingsAccountRepositoryMock.Verify(x => x.UpdateAsync(cuentaOrigen), Times.Once);
        _transactionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>()), Times.Once);
    }

    #endregion

    #region Pruebas de TransferBetweenAccountsAsync

    [Fact]
    public async Task TransferBetweenAccountsAsync_MontoInvalido_DeberiaLanzarExcepcion()
    {
        // Arrange
        var userId = "user123";
        var dto = new TransferBetweenAccountsDTO
        {
            CuentaOrigen = "12345",
            CuentaDestino = "67890",
            Monto = 0
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _transactionService.TransferBetweenAccountsAsync(userId, dto));
    }

    #endregion

    #region Pruebas de PayCreditCardAsync

    [Fact]
    public async Task PayCreditCardAsync_MontoInvalido_DeberiaLanzarExcepcion()
    {
        // Arrange
        var userId = "user123";
        var dto = new PayCreditCardDTO
        {
            CuentaOrigen = "12345",
            Monto = -50,
            UserId = userId,
            CardNumber = "1234567890123456"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _transactionService.PayCreditCardAsync(userId, dto));
    }

    #endregion
}
