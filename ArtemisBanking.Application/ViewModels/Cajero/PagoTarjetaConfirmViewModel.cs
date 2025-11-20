using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    public class PagoTarjetaConfirmViewModel
    {
        [Display(Name = "Cuenta origen")]
        public string CuentaOrigen { get; set; } = default!;

        [Display(Name = "Cuenta origen")]
        public string CuentaOrigenEnmascarada { get; set; } = default!;

        [Display(Name = "Balance actual cuenta origen")]
        public decimal BalanceActualCuentaOrigen { get; set; }

        public int CreditCardId { get; set; }

        [Display(Name = "Tarjeta de crédito")]
        public string NumeroTarjetaEnmascarada { get; set; } = default!;

        [Display(Name = "Titular de la tarjeta")]
        public string TitularTarjeta { get; set; } = default!;

        [Display(Name = "Deuda actual")]
        public decimal DeudaActual { get; set; }

        [Display(Name = "Monto solicitado")]
        public decimal MontoSolicitado { get; set; }

        [Display(Name = "Monto que se aplicará realmente")]
        public decimal MontoAplicadoReal { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
