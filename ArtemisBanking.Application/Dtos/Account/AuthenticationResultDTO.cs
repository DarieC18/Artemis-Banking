namespace ArtemisBanking.Application.DTOs.Account
{
    public class AuthenticationResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserDTO User { get; set; }
    }
}
