using System;

namespace ArtemisBanking.Application.Dtos.Identity
{
    public class CreateIdentityUserCommand
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime FechaCreacionUtc { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}
