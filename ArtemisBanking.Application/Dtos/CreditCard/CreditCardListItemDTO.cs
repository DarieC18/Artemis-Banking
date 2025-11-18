namespace ArtemisBanking.Application.Dtos.CreditCard
{
    public class CreditCardListItemDTO
    {
        public int Id { get; set; }
        public string NumeroTarjeta { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public decimal LimiteCredito { get; set; }
        public string FechaExpiracion { get; set; } = string.Empty; // Formato MM/yy
        public decimal DeudaActual { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
