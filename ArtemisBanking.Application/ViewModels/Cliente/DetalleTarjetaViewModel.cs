namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class DetalleTarjetaViewModel
    {
        public int TarjetaId { get; set; }
        public string NumeroTarjeta { get; set; } = string.Empty;
        public string Ultimos4Digitos { get; set; } = string.Empty;

        public decimal LimiteCredito { get; set; }
        public decimal DeudaActual { get; set; }
        public decimal CreditoDisponible { get; set; }
        public string FechaExpiracion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }

        public List<ConsumoTarjetaViewModel> Consumos { get; set; } = new();
    }

    public class ConsumoTarjetaViewModel
    {
        public DateTime FechaConsumo { get; set; }
        public decimal Monto { get; set; }
        public string Comercio { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
