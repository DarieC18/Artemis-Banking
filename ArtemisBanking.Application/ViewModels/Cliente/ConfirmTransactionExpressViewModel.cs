using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class ConfirmTransactionExpressViewModel
    {
        [Required]
        public string CuentaOrigen { get; set; }

        [Required]
        public string CuentaDestino { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Monto { get; set; }
        public string NombreTitularDestino { get; set; }

    }
}
