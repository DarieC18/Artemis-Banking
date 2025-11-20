namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class DetalleCuentaViewModel
    {
        public int CuentaId { get; set; }
        public string NumeroCuenta { get; set; } = string.Empty;
        public decimal BalanceActual { get; set; }
        public bool EsPrincipal { get; set; }

        public List<TransaccionDetalleViewModel> Transacciones { get; set; }
            = new List<TransaccionDetalleViewModel>();
    }

    public class TransaccionDetalleViewModel
    {
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Beneficiario { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
