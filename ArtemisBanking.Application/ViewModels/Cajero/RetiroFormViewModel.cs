using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    public class RetiroFormViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es obligatorio.")]
        [Display(Name = "Número de cuenta origen")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        [Display(Name = "Monto a retirar")]
        public decimal Amount { get; set; }
    }
}
