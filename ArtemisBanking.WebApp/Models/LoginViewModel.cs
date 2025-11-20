using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.WebApp.Models
{
    /// <summary>
    /// ViewModel para el formulario de inicio de sesión
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [Display(Name = "Usuario")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
