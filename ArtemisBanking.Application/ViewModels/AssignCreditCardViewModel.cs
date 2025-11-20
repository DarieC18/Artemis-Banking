using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels
{
    public class AssignCreditCardViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El límite de crédito debe ser mayor a 0")]
        public decimal LimiteCredito { get; set; }
    }
}
