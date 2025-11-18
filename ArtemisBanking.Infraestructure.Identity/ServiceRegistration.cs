using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Settings;
using ArtemisBanking.Infraestructure.Identity.Context;
using ArtemisBanking.Infraestructure.Identity.Entities;
using ArtemisBanking.Infraestructure.Identity.Seeds;
using ArtemisBanking.Infraestructure.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ArtemisBanking.Infraestructure.Identity
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            #region Database
            services.AddDbContext<IdentityContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("IdentityConnection"),
                    b => b.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)));
            #endregion

            #region Identity
            var identityBuilder = services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            });

            identityBuilder = new IdentityBuilder(identityBuilder.UserType, typeof(IdentityRole), identityBuilder.Services);
            identityBuilder.AddRoles<IdentityRole>();
            identityBuilder.AddEntityFrameworkStores<IdentityContext>();
            identityBuilder.AddSignInManager<SignInManager<AppUser>>();
            identityBuilder.AddRoleManager<RoleManager<IdentityRole>>();
            identityBuilder.AddDefaultTokenProviders();
            #endregion

            #region Cookie Authentication (WebApp)
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });
            #endregion

            #region Authorization
            services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Administrador"));
                options.AddPolicy("RequireCajeroRole", policy => policy.RequireRole("Cajero"));
                options.AddPolicy("RequireClienteRole", policy => policy.RequireRole("Cliente"));
            });
            #endregion

            #region Services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAccountServiceApi, AccountServiceApi>();
            services.AddScoped<IUserInfoService, UserInfoService>();

            return services;
        }
        #endregion

        public static IServiceCollection AddIdentityInfrastructureForApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityInfrastructure(configuration);

            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            });

            return services;
        }

        public static async Task SeedIdentityAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            await DefaultRoles.SeedAsync(roleManager);

            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            await DefaultUsers.SeedAsync(userManager);
        }
    }
}
