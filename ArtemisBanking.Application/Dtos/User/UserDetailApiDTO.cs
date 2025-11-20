namespace ArtemisBanking.Application.DTOs.Users
{
    public class UserDetailApiDto
    {
        public string Usuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cedula { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }
        public string Estado { get; set; }

        public CuentaPrincipalDto CuentaPrincipal { get; set; }
    }

    public class CuentaPrincipalDto
    {
        public string NumeroCuenta { get; set; }
        public decimal Balance { get; set; }
    }
}
