using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.Mappings.EntitiesAndDtos;
using ArtemisBanking.Application.Mappings.DtosAndViewModels;
using ArtemisBanking.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBanking.Application
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayerIoc(this IServiceCollection services, IConfiguration configuration)
        {
            // Registra los perfiles de EntitiesAndDtos y DtosAndViewModels
            services.AddAutoMapper(
                typeof(SavingsAccountProfile).Assembly,
                typeof(AdminUserProfile).Assembly);

            services.AddScoped<IClienteHomeService, ClienteHomeService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IBeneficiaryService, BeneficiaryService>();
            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IAdminLoanService, AdminLoanService>();
            services.AddScoped<IAdminCreditCardService, AdminCreditCardService>();
            services.AddScoped<IAdminSavingsAccountService, AdminSavingsAccountService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        }
    }
}
