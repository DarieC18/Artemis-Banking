using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    public class DepositoFormViewModel
    {
        [Required(ErrorMessage = "El número de cuenta destino es obligatorio.")]
        [Display(Name = "Número de cuenta destino")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        [Display(Name = "Monto a depositar")]
        public decimal Amount { get; set; }
    }
}
