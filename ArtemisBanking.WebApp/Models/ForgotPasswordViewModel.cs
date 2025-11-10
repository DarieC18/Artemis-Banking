using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.WebApp.Models
{
    /// <summary>
    /// ViewModel para solicitar restablecimiento de contraseña
    /// </summary>
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [Display(Name = "Usuario")]
        public string UserName { get; set; }
    }
}
