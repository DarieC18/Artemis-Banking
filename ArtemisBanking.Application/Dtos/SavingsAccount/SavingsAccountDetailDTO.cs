using ArtemisBanking.Application.Dtos.Transaction;

namespace ArtemisBanking.Application.Dtos.SavingsAccount
{
    public class SavingsAccountDetailDTO
    {
        public string NumeroCuenta { get; set; }
        public decimal Balance { get; set; }
        public bool EsPrincipal { get; set; }
        public string TipoCuenta { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<TransactionDTO> Transacciones { get; set; }
    }
}