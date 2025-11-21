using ArtemisBanking.Domain.Enums;
using ArtemisBanking.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace ArtemisBanking.Infraestructure.Identity.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            // Usuario Administrador
            var adminUser = new AppUser
            {
                UserName = "admin",
                Email = "admin@artemisbanking.com",
                EmailConfirmed = true,
                Cedula = "00100000001",
                Nombre = "Admin",
                Apellido = "Sistema",
                IsActive = true,
                FechaCreacion = DateTime.Now
            };

            if (userManager.Users.All(u => u.UserName != adminUser.UserName))
            {
                var user = await userManager.FindByNameAsync(adminUser.UserName);
                if (user == null)
                {
                    await userManager.CreateAsync(adminUser, "Admin123!");
                    await userManager.AddToRoleAsync(adminUser, Roles.Administrador.ToString());
                }
            }

            // Usuario Cajero
            var cajeroUser = new AppUser
            {
                UserName = "cajero",
                Email = "cajero@artemisbanking.com", 
                EmailConfirmed = true,
                Cedula = "00200000002",
                Nombre = "Juan",
                Apellido = "Pérez",
                IsActive = true,
                FechaCreacion = DateTime.Now
            };

            if (userManager.Users.All(u => u.UserName != cajeroUser.UserName))
            {
                var user = await userManager.FindByNameAsync(cajeroUser.UserName);
                if (user == null)
                {
                    await userManager.CreateAsync(cajeroUser, "Cajero123!");
                    await userManager.AddToRoleAsync(cajeroUser, Roles.Cajero.ToString());
                }
            }

            // Usuario Cliente
            var clienteUser = new AppUser
            {
                UserName = "cliente1",
                Email = "cliente1@artemisbanking.com",
                EmailConfirmed = true,
                Cedula = "00300000003",
                Nombre = "cliente",
                Apellido = "ej",
                IsActive = true,
                FechaCreacion = DateTime.Now
            };

            if (userManager.Users.All(u => u.UserName != clienteUser.UserName))
            {
                var user = await userManager.FindByNameAsync(clienteUser.UserName);
                if (user == null)
                {
                    await userManager.CreateAsync(clienteUser, "Cliente123!");
                    await userManager.AddToRoleAsync(clienteUser, Roles.Cliente.ToString());
                }
            }
        }
    }
}
