namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class DetallePrestamoViewModel
    {
        public int PrestamoId { get; set; }
        public string NumeroPrestamo { get; set; } = string.Empty;

        public decimal MontoCapital { get; set; }
        public int CuotasTotales { get; set; }
        public int CuotasPagadas { get; set; }
        public decimal MontoPendiente { get; set; }
        public decimal TasaInteres { get; set; }
        public int PlazoMeses { get; set; }
        public string EstadoPago { get; set; } = string.Empty;

        public List<CuotaPrestamoViewModel> Cuotas { get; set; } = new();
    }

    public class CuotaPrestamoViewModel
    {
        public int NumeroCuota { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal ValorCuota { get; set; }
        public bool Pagada { get; set; }
        public bool Atrasada { get; set; }
    }
}
