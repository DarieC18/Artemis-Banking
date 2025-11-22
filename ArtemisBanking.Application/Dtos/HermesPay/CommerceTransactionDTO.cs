namespace ArtemisBanking.Application.DTOs.Hermes
{
    public class CommerceTransactionDto
    {
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Beneficiario { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
