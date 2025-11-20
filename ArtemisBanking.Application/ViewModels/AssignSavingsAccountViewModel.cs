using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels
{
    public class AssignSavingsAccountViewModel
    {
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Balance inicial")]
        [Range(0, double.MaxValue, ErrorMessage = "El balance inicial no puede ser negativo")]
        public decimal BalanceInicial { get; set; } = 0;
    }
}

