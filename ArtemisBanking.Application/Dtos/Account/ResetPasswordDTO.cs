namespace ArtemisBanking.Application.DTOs.Account
{
    public class ResetPasswordDTO
    {
        public string UserName { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
    }
}
