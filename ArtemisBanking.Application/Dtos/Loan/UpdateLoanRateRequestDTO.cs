using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.Dtos.Loan
{
    public class UpdateLoanRateRequestDTO
    {
        [Required(ErrorMessage = "La tasa de interés es requerida")]
        [Range(0, 100, ErrorMessage = "La tasa de interés debe estar entre 0 y 100")]
        public decimal TasaInteres { get; set; }
    }
}

