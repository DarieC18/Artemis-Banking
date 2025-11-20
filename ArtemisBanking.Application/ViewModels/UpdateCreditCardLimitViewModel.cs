using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels
{
    public class UpdateCreditCardLimitViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El límite de crédito debe ser mayor a 0")]
        public decimal LimiteCredito { get; set; }
    }
}
