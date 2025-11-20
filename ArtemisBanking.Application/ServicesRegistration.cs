using ArtemisBanking.Application.Interfaces.Services;
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
            services.AddAutoMapper(typeof(SavingsAccountProfile).Assembly);

            services.AddScoped<IClienteHomeService, ClienteHomeService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IBeneficiaryService, BeneficiaryService>();
            services.AddScoped<IClienteProductoService, ClienteProductoService>();
            services.AddScoped<IAccountCashOperationsService, AccountCashOperationsService>();
            services.AddScoped<ILoanPaymentService, LoanPaymentService>();
            services.AddScoped<ICreditCardPaymentService, CreditCardPaymentService>();
            services.AddScoped<ICashierThirdPartyTransferService, CashierThirdPartyTransferService>();
            services.AddScoped<ICommerceServiceApi, CommerceServiceApi>();
            services.AddScoped<IHermesPayService, HermesPayService>();
        }
    }
}
