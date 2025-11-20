namespace ArtemisBanking.Application.DTOs.Account
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Cedula { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public bool IsActive { get; set; }
        public DateTime FechaCreacion { get; set; }
        public IList<string> Roles { get; set; }
    }
}
