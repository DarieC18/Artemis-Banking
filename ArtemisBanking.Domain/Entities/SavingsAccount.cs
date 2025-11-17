namespace ArtemisBanking.Domain.Entities
{
    public class SavingsAccount
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; }
        public decimal Balance { get; set; }
        public bool EsPrincipal { get; set; }
        public bool IsActive { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UserId { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();
    }
}