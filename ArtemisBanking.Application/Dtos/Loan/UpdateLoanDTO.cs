using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.Dtos.Loan
{
    public class UpdateLoanDTO
    {
        [Required]
        public int Id { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? MontoCapital { get; set; }

        [Range(0, 100)]
        public decimal? TasaInteres { get; set; }

        [Range(6, 60)]
        public int? PlazoMeses { get; set; }
    }
}

