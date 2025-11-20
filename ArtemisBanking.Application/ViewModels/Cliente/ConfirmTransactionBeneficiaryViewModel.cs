using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class ConfirmTransactionBeneficiaryViewModel
    {
        [Required]
        public int BeneficiaryId { get; set; }

        [Required]
        public string BeneficiaryNombre { get; set; }

        [Required]
        public string NumeroCuentaBeneficiario { get; set; }

        [Required]
        public string CuentaOrigen { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Monto { get; set; }
    }
}
