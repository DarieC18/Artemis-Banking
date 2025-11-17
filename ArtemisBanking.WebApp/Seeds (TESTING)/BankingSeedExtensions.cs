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

            var clienteUser = await userManager.FindByNameAsync("cliente1");
            if (clienteUser == null)
                return;

            var cuentaExistente = await context.SavingsAccounts
                .FirstOrDefaultAsync(c => c.UserId == clienteUser.Id && c.EsPrincipal);

            if (cuentaExistente != null)
                return;

            var cuenta = new SavingsAccount
            {
                NumeroCuenta = "001000001",
                Balance = 10000m,
                EsPrincipal = true,
                IsActive = true,
                FechaCreacion = DateTime.UtcNow,
                UserId = clienteUser.Id
            };

            context.SavingsAccounts.Add(cuenta);
            await context.SaveChangesAsync();

            var transaccionApertura = new Transaction
            {
                SavingsAccountId = cuenta.Id,
                Monto = 10000m,
                FechaTransaccion = DateTime.UtcNow,
                Tipo = "DEPOSITO_INICIAL",
                Beneficiario = "CUENTA_PROPIA",
                Origen = "APERTURA",
                Estado = "COMPLETADA"
            };

            context.Transactions.Add(transaccionApertura);
            await context.SaveChangesAsync();
        }
    }
}
