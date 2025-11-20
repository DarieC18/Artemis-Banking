namespace ArtemisBanking.Application.DTOs.Users
{
    public class UpdateUserApiDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cedula { get; set; }
        public string Correo { get; set; }
        public string Usuario { get; set; }
        public string Contrasena { get; set; }
        public string ConfirmarContrasena { get; set; }
        public decimal? MontoAdicional { get; set; }
    }
}
