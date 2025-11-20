using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ArtemisBanking.Infrastructure.Persistence;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Configura el DbContext
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection") 
            ?? context.Configuration["Values:DefaultConnection"];
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<ArtemisBankingDbContext>(options =>
                options.UseSqlServer(connectionString));
        }
    })
    .Build();

host.Run();
