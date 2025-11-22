using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Account
{
    public class ConfirmAccountApiRequestDTO
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
