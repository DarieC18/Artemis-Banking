using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ArtemisBanking.Infrastructure.Persistence;
using ArtemisBanking.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBank.CuotasJob
{
    public class CuotasAtrasadas
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public CuotasAtrasadas(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<CuotasAtrasadas>();
            _serviceProvider = serviceProvider;
        }

        [Function("CuotasAtrasadas")]
        public async Task Run([TimerTrigger("0 1 0 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ArtemisBankingDbContext>();

                var today = DateTime.UtcNow.Date;
                _logger.LogInformation($"Procesando cuotas atrasadas para la fecha: {today:yyyy-MM-dd}");

                // Obtiene todas las cuotas que no estan pagadas y cuya fecha de pago ya paso
                var cuotasAtrasadas = await context.LoanPaymentSchedules
                    .Where(c => !c.Pagada && c.FechaPago < today && !c.Atrasada)
                    .ToListAsync();

                _logger.LogInformation($"Encontradas {cuotasAtrasadas.Count} cuotas que deben marcarse como atrasadas");

                // la marca como atrasadas
                foreach (var cuota in cuotasAtrasadas)
                {
                    cuota.Atrasada = true;
                    _logger.LogInformation($"Cuota {cuota.NumeroCuota} del préstamo {cuota.LoanId} marcada como atrasada (Fecha pago: {cuota.FechaPago:yyyy-MM-dd})");
                }

                // actualiza las cuotas que fueron pagadas pero aun estan marcadas como atrasadas
                var cuotasPagadasPeroAtrasadas = await context.LoanPaymentSchedules
                    .Where(c => c.Pagada && c.Atrasada)
                    .ToListAsync();

                _logger.LogInformation($"Encontradas {cuotasPagadasPeroAtrasadas.Count} cuotas pagadas que deben quitarse de atrasadas");

                foreach (var cuota in cuotasPagadasPeroAtrasadas)
                {
                    cuota.Atrasada = false;
                    _logger.LogInformation($"Cuota {cuota.NumeroCuota} del préstamo {cuota.LoanId} removida de atrasadas (ya fue pagada)");
                }

                if (cuotasAtrasadas.Any() || cuotasPagadasPeroAtrasadas.Any())
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"Proceso completado. {cuotasAtrasadas.Count} cuotas marcadas como atrasadas, {cuotasPagadasPeroAtrasadas.Count} cuotas removidas de atrasadas");
                }
                else
                {
                    _logger.LogInformation("No se encontraron cuotas que requieran actualización");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar cuotas atrasadas");
                throw;
            }
        }
    }
}
