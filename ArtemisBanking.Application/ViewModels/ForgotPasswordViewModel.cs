using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [Display(Name = "Usuario")]
        public string UserName { get; set; }
    }
}
