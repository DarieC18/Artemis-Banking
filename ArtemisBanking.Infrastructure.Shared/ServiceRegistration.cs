using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Settings;
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
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            //services.AddTransient<IEmailService, FakeEmailService>(); //Testing, cambiar a EmailService
            services.AddTransient<IEmailService, EmailService>();
            services.AddSingleton(TimeProvider.System);
            return services;
        }
    }
}
