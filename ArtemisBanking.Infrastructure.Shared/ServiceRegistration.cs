using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Infrastructure.Shared.Services;
using ArtemisBanking.Infrastructure.Shared.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBanking.Infrastructure.Shared
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            IServiceCollection serviceCollection = services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.AddScoped<IEmailService, EmailService>();
            return services;
        }
    }
}
