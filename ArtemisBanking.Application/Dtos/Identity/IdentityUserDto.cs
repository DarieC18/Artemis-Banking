using System;
using System.Collections.Generic;

namespace ArtemisBanking.Application.Dtos.Identity
{
    public class IdentityUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime FechaCreacion { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
