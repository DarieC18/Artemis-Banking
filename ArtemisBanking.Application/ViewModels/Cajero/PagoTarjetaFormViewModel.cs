using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    public class PagoTarjetaFormViewModel
    {
        [Required(ErrorMessage = "La cuenta de origen es obligatoria.")]
        [Display(Name = "Cuenta origen")]
        public string CuentaOrigen { get; set; } = string.Empty;

        [Required(ErrorMessage = "La tarjeta es obligatoria.")]
        [Display(Name = "ID de tarjeta de crédito")]
        public int CreditCardId { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        [Display(Name = "Monto a pagar")]
        public decimal Monto { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
