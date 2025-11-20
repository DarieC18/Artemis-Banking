namespace ArtemisBanking.Application.Dtos.CreditCard
{
    public class CreditCardDetailDTO
    {
        public string NumeroTarjeta { get; set; }
        public string Ultimos4Digitos { get; set; }
        public decimal LimiteCredito { get; set; }
        public decimal DeudaActual { get; set; }
        public decimal CreditoDisponible { get; set; }
        public string FechaExpiracion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<CreditCardConsumptionDTO> Consumos { get; set; }
    }
}