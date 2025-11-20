using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.Dtos.AdminUsers
{
    public class UpdateAdminUserDTO
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        public string Cedula { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string? ConfirmPassword { get; set; }

        public decimal? MontoAdicional { get; set; }
    }
}
