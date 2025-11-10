using Microsoft.AspNetCore.Identity;

namespace ArtemisBanking.Infraestructure.Identity.Entities
{
    public class AppUser : IdentityUser
    {
        public string Cedula { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public bool IsActive { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }
    }
}
