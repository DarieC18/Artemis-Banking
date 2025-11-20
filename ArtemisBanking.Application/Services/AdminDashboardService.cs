using ArtemisBanking.Application.Dtos.AdminDashboard;
using ArtemisBanking.Application.Interfaces.Identity;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;

namespace ArtemisBanking.Application.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IIdentityUserManager _identityUserManager;
        private readonly ILoanRepository _loanRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ITransactionRepository _transactionRepository;

        public AdminDashboardService(
            IIdentityUserManager identityUserManager,
            ILoanRepository loanRepository,
            ICreditCardRepository creditCardRepository,
            ISavingsAccountRepository savingsAccountRepository,
            ITransactionRepository transactionRepository)
        {
            _identityUserManager = identityUserManager;
            _loanRepository = loanRepository;
            _creditCardRepository = creditCardRepository;
            _savingsAccountRepository = savingsAccountRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<AdminDashboardSummaryDTO> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var today = DateTime.UtcNow.Date;
            var todayEnd = today.AddDays(1);

            // Transacciones totales
            var totalTransactions = await _transactionRepository.GetTotalCountAsync();

            // Transacciones de hoy
            var todayTransactions = await _transactionRepository.GetTodayCountAsync(today, todayEnd);

            // Pagos procesados hoy (transacciones de tipo "Pago de préstamo" o "Pago de tarjeta")
            var todayPayments = await _transactionRepository.GetTodayPaymentsCountAsync(today, todayEnd);

            // Total pagos históricos
            var totalPayments = await _transactionRepository.GetTotalPaymentsCountAsync();

            // Clientes activos e inactivos
            var allUsers = await _identityUserManager.GetAllAsync(cancellationToken);
            var activeClients = allUsers.Count(u => u.IsActive && u.Roles.Contains("Cliente"));
            var inactiveClients = allUsers.Count(u => !u.IsActive && u.Roles.Contains("Cliente"));

            // Préstamos vigentes
            var activeLoans = await _loanRepository.GetAllActiveAsync();
            var activeLoansCount = activeLoans.Count;

            // Tarjetas activas
            var allCreditCards = await _creditCardRepository.GetAllAsync();
            var activeCreditCards = allCreditCards.Count(c => c.IsActive);

            // Cuentas de ahorro abiertas
            var allSavingsAccounts = await _savingsAccountRepository.GetAllAsync();
            var openSavingsAccounts = allSavingsAccounts.Count;

            // Productos financieros totales (cuentas + préstamos + tarjetas)
            var totalProducts = openSavingsAccounts + activeLoansCount + allCreditCards.Count;

            // Monto promedio de deuda por cliente
            var averageDebtPerClient = await _loanRepository.GetAverageDebtAsync();

            return new AdminDashboardSummaryDTO
            {
                TotalTransactions = totalTransactions,
                TodayTransactions = todayTransactions,
                TodayPayments = todayPayments,
                TotalPayments = totalPayments,
                ActiveClients = activeClients,
                InactiveClients = inactiveClients,
                TotalProducts = totalProducts,
                ActiveLoans = activeLoansCount,
                ActiveCreditCards = activeCreditCards,
                OpenSavingsAccounts = openSavingsAccounts,
                AverageDebtPerClient = averageDebtPerClient
            };
        }
    }
}

