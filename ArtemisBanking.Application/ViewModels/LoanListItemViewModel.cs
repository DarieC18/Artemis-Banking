namespace ArtemisBanking.Application.ViewModels
{
    public class LoanListItemViewModel
    {
        public int Id { get; set; }
        public string NumeroPrestamo { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public decimal MontoCapital { get; set; }
        public int CuotasTotales { get; set; }
        public int CuotasPagadas { get; set; }
        public decimal MontoPendiente { get; set; }
        public decimal TasaInteres { get; set; }
        public int PlazoMeses { get; set; }
        public string EstadoPago { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

