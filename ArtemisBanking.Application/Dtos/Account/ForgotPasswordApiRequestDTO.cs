using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Account
{
    public class ForgotPasswordApiRequestDTO
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
    }
}
