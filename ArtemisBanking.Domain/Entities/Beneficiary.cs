namespace ArtemisBanking.Domain.Entities
{
    public class Beneficiary
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string NumeroCuentaBeneficiario { get; set; }
        public string NombreBeneficiario { get; set; }
        public string ApellidoBeneficiario { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}