using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels
{
    public class AssignLoanViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(6, 60, ErrorMessage = "El plazo debe estar entre 6 y 60 meses")]
        public int PlazoMeses { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal MontoCapital { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "La tasa de interés debe estar entre 0 y 100")]
        public decimal TasaInteres { get; set; }
    }
}

