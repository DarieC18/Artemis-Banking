namespace ArtemisBanking.Application.Dtos.Beneficiary
{
    public class BeneficiaryDTO
    {
        public int Id { get; set; }
        public string NumeroCuentaBeneficiario { get; set; }
        public string NombreBeneficiario { get; set; }
        public string ApellidoBeneficiario { get; set; }
        public string NombreCompleto
        {
            get
            {
                var nombre = NombreBeneficiario?.Trim() ?? string.Empty;
                var apellido = ApellidoBeneficiario?.Trim() ?? string.Empty;

                var full = $"{nombre} {apellido}".Trim();
                return string.IsNullOrWhiteSpace(full) ? nombre : full;
            }
        }
    }
}