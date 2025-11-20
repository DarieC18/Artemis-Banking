using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.Dtos.Loan
{
    public class AssignLoanRequestDTO
    {
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El plazo en meses es requerido")]
        [Range(6, 60, ErrorMessage = "El plazo debe estar entre 6 y 60 meses")]
        public int PlazoMeses { get; set; }

        [Required(ErrorMessage = "El monto capital es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto capital debe ser mayor a cero")]
        public decimal MontoCapital { get; set; }

        [Required(ErrorMessage = "La tasa de interés es requerida")]
        [Range(0, 100, ErrorMessage = "La tasa de interés debe estar entre 0 y 100")]
        public decimal TasaInteres { get; set; }
    }
}

