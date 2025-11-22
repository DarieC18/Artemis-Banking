namespace ArtemisBanking.Application.DTOs.Users
{
    public class CreateUserApiDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cedula { get; set; }
        public string Correo { get; set; }
        public string Usuario { get; set; }
        public string Contrasena { get; set; }
        public string ConfirmarContrasena { get; set; }
        public string TipoUsuario { get; set; }
        public decimal? MontoInicial { get; set; }
    }
}
