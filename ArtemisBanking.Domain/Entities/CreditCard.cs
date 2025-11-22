namespace ArtemisBanking.Domain.Entities
{
    public class CreditCard
    {
        public int Id { get; set; }
        public string NumeroTarjeta { get; set; }
        public decimal LimiteCredito { get; set; }
        public decimal DeudaActual { get; set; }
        public string FechaExpiracion { get; set; }
        public string CVCHash { get; set; }
        public bool IsActive { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UserId { get; set; }
        public string AdminUserId { get; set; }

        public virtual ICollection<CreditCardConsumption> Consumos { get; set; }
    }
}