using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    public class PagoPrestamoFormViewModel
    {
        [Required(ErrorMessage = "La cuenta origen es requerida.")]
        [Display(Name = "Cuenta origen")]
        public string CuentaOrigen { get; set; } = string.Empty;

        [Required(ErrorMessage = "El préstamo es requerido.")]
        [Display(Name = "Id del préstamo")]
        public int LoanId { get; set; }

        [Required]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de préstamo debe tener 9 dígitos.")]
        public string LoanNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto a pagar es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        [Display(Name = "Monto a pagar")]
        public decimal Monto { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
