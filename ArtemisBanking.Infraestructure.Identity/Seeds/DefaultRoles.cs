using ArtemisBanking.Domain.Enums;
using ArtemisBanking.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace ArtemisBanking.Infraestructure.Identity.Seeds
{

    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            // Crea el rol Administrador
            if (!await roleManager.RoleExistsAsync(Roles.Administrador.ToString()))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Administrador.ToString()));
            }

            // Crea el rol Cajero
            if (!await roleManager.RoleExistsAsync(Roles.Cajero.ToString()))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Cajero.ToString()));
            }

            // Crea el rol Cliente
            if (!await roleManager.RoleExistsAsync(Roles.Cliente.ToString()))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Cliente.ToString()));
            }

            // Crea el rol Comercio
            if (!await roleManager.RoleExistsAsync(Roles.Comercio.ToString()))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Comercio.ToString()));
            }
        }
    }
}
