namespace ArtemisBanking.Application.Dtos.SavingsAccount
{
    public class AssignSavingsAccountDTO
    {
        public string UserId { get; set; } = string.Empty;
        public decimal BalanceInicial { get; set; }
    }
}

