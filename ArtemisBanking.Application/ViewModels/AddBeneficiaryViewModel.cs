using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels
{
    public class AddBeneficiaryViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es requerido")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de cuenta del beneficiario")]
        public string NumeroCuentaBeneficiario { get; set; }
    }
}