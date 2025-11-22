using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    public class DepositoConfirmViewModel
    {
        [Display(Name = "Número de cuenta destino")]
        public string AccountNumber { get; set; } = string.Empty;

        [Display(Name = "Cuenta destino")]
        public string AccountMasked { get; set; } = string.Empty;

        [Display(Name = "Titular")]
        public string HolderFullName { get; set; } = string.Empty;

        [Display(Name = "Monto a depositar")]
        public decimal Amount { get; set; }

        [Display(Name = "Balance actual")]
        public decimal CurrentBalance { get; set; }
    }
}
