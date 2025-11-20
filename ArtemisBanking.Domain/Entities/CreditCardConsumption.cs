namespace ArtemisBanking.Domain.Entities
{
    public class CreditCardConsumption
    {
        public int Id { get; set; }
        public int CreditCardId { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaConsumo { get; set; }
        public string Comercio { get; set; }
        public string Estado { get; set; }
        public virtual CreditCard CreditCard { get; set; }
        public bool EsAvanceEfectivo { get; set; }

    }
}