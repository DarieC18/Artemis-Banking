using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infraestructure.Identity.Entities;
using ArtemisBanking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.WebApp.Seed
{
    public static class BankingSeedExtensions
    {
        public static async Task SeedBankingDataAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ArtemisBankingDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            // Buscar al usuario cliente1
            var clienteUser = await userManager.FindByNameAsync("cliente1");
            if (clienteUser == null)
                return;

            // ============================================================
            // 1. Cuenta principal del cliente
            // ============================================================
            var cuentaPrincipal = await context.SavingsAccounts
                .FirstOrDefaultAsync(c => c.UserId == clienteUser.Id && c.EsPrincipal);

            if (cuentaPrincipal == null)
            {
                cuentaPrincipal = new SavingsAccount
                {
                    NumeroCuenta = "001000001",
                    Balance = 10000m,
                    EsPrincipal = true,
                    IsActive = true,
                    FechaCreacion = DateTime.UtcNow,
                    UserId = clienteUser.Id
                };

                context.SavingsAccounts.Add(cuentaPrincipal);
                await context.SaveChangesAsync();
            }

            // ============================================================
            // 2. Segunda cuenta para transferencias a terceros
            // ============================================================
            var cuentaSecundaria = await context.SavingsAccounts
                .FirstOrDefaultAsync(c =>
                    c.UserId == clienteUser.Id &&
                    !c.EsPrincipal &&
                    c.NumeroCuenta == "001000002");

            if (cuentaSecundaria == null)
            {
                cuentaSecundaria = new SavingsAccount
                {
                    NumeroCuenta = "001000002",
                    Balance = 5000m,
                    EsPrincipal = false,
                    IsActive = true,
                    FechaCreacion = DateTime.UtcNow,
                    UserId = clienteUser.Id
                };

                context.SavingsAccounts.Add(cuentaSecundaria);
                await context.SaveChangesAsync();
            }

            // ============================================================
            // 3. Tarjeta de crédito dummy
            // ============================================================
            var creditCard = await context.CreditCards
                .FirstOrDefaultAsync(c => c.UserId == clienteUser.Id);

            if (creditCard == null)
            {
                creditCard = new CreditCard
                {
                    UserId = clienteUser.Id,
                    AdminUserId = "admin",
                    NumeroTarjeta = "4111111111111111",
                    LimiteCredito = 20000m,
                    DeudaActual = 3500m,
                    FechaExpiracion = "12/29",
                    CVCHash = "hashed-cvc-123",
                    IsActive = true,
                    FechaCreacion = DateTime.UtcNow,
                    Consumos = new List<CreditCardConsumption>()
                };

                context.CreditCards.Add(creditCard);
                await context.SaveChangesAsync();

                // Consumos dummy
                var consumo1 = new CreditCardConsumption
                {
                    CreditCardId = creditCard.Id,
                    Monto = 1500m,
                    FechaConsumo = DateTime.UtcNow.AddDays(-3),
                    Comercio = "Supermercado Nacional",
                    Estado = "APROBADO",
                    EsAvanceEfectivo = false
                };

                var consumo2 = new CreditCardConsumption
                {
                    CreditCardId = creditCard.Id,
                    Monto = 2000m,
                    FechaConsumo = DateTime.UtcNow.AddDays(-1),
                    Comercio = "Ferretería Americana",
                    Estado = "APROBADO",
                    EsAvanceEfectivo = false
                };

                context.CreditCardConsumptions.Add(consumo1);
                context.CreditCardConsumptions.Add(consumo2);
                await context.SaveChangesAsync();
            }
            // ============================================================
            // 4. Préstamo dummy
            // ============================================================
            var loan = await context.Loans
                .FirstOrDefaultAsync(l => l.UserId == clienteUser.Id);

            if (loan == null)
            {
                loan = new Loan
                {
                    UserId = clienteUser.Id,
                    AdminUserId = "admin",
                    NumeroPrestamo = "000123456",
                    MontoCapital = 50000m,
                    CuotasTotales = 12,
                    CuotasPagadas = 0,
                    EstadoPago = "Al día",
                    MontoPendiente = 50000m,
                    TasaInteres = 0.18m,
                    PlazoMeses = 12,
                    IsActive = true,
                    FechaCreacion = DateTime.UtcNow
                };

                context.Loans.Add(loan);
                await context.SaveChangesAsync();

                // ============================================================
                // 4.1 Tabla de amortización dummy CORREGIDA
                // ============================================================
                decimal cuotaMensual = Math.Round(loan.MontoCapital / loan.CuotasTotales, 2);

                decimal totalPendiente = 0m;

                for (int i = 1; i <= loan.CuotasTotales; i++)
                {
                    var schedule = new LoanPaymentSchedule
                    {
                        LoanId = loan.Id,
                        NumeroCuota = i,
                        ValorCuota = cuotaMensual,
                        SaldoPendiente = cuotaMensual,
                        FechaPago = DateTime.UtcNow.AddMonths(i),
                        Pagada = false,
                        Atrasada = false
                    };

                    totalPendiente += cuotaMensual;

                    context.LoanPaymentSchedules.Add(schedule);
                }

                await context.SaveChangesAsync();

                loan.MontoPendiente = totalPendiente;
                context.Loans.Update(loan);
                await context.SaveChangesAsync();

                // ============================================================
                // 4.2 Desembolso del préstamo → acreditarlo a la cuenta principal
                // ============================================================
                cuentaPrincipal.Balance += loan.MontoCapital;
                context.SavingsAccounts.Update(cuentaPrincipal);
                await context.SaveChangesAsync();

                var loanTx = new Transaction
                {
                    SavingsAccountId = cuentaPrincipal.Id,
                    Monto = loan.MontoCapital,
                    FechaTransaccion = DateTime.UtcNow,
                    Tipo = "CRÉDITO",
                    Beneficiario = cuentaPrincipal.NumeroCuenta,
                    Origen = "000123456",
                    Estado = "APROBADA",
                    OperatedByUserId = clienteUser.Id
                };

                context.Transactions.Add(loanTx);
                await context.SaveChangesAsync();
            }

        }
    }
}
