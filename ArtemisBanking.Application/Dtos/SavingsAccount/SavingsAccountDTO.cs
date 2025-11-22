namespace ArtemisBanking.Application.Dtos.SavingsAccount
{
    public class SavingsAccountDTO
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; }
        public decimal Balance { get; set; }
        public bool EsPrincipal { get; set; }
        public string TipoCuenta { get; set; }
    }
}