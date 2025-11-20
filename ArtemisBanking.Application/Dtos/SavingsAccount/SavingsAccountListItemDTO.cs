namespace ArtemisBanking.Application.Dtos.SavingsAccount
{
    public class SavingsAccountListItemDTO
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public string ClienteCedula { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string TipoCuenta { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool EsPrincipal { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}

