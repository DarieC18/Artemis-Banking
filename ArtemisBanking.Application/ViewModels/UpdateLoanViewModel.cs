using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels
{
    public class UpdateLoanViewModel
    {
        [Required]
        public int Id { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal? MontoCapital { get; set; }

        [Range(0, 100, ErrorMessage = "La tasa de interés debe estar entre 0 y 100")]
        public decimal? TasaInteres { get; set; }

        [Range(6, 60, ErrorMessage = "El plazo debe estar entre 6 y 60 meses")]
        public int? PlazoMeses { get; set; }
    }
}

