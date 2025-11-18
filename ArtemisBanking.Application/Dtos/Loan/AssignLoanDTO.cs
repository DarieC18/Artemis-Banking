using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.Dtos.Loan
{
    public class AssignLoanDTO
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(6, 60)]
        public int PlazoMeses { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal MontoCapital { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal TasaInteres { get; set; }
    }
}

