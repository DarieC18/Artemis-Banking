namespace ArtemisBanking.Application.Dtos.CreditCard
{
    public class CreditCardConsumptionDTO
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaConsumo { get; set; }
        public string Comercio { get; set; }
        public string Estado { get; set; }  // APROBADO o RECHAZADO
    }
}