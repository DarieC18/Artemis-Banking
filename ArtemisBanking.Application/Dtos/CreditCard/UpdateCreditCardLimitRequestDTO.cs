using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.Dtos.CreditCard
{
    public class UpdateCreditCardLimitRequestDTO
    {
        [Required(ErrorMessage = "El límite de crédito es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El límite de crédito debe ser mayor a cero")]
        public decimal LimiteCredito { get; set; }
    }
}

