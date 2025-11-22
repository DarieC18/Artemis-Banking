namespace ArtemisBanking.Application.Dtos.CreditCard
{
    public class ClientForCreditCardDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal DeudaTotal { get; set; }
    }
}
