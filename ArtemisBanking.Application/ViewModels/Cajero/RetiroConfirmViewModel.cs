using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    public class RetiroConfirmViewModel
    {
        [Display(Name = "Número de cuenta origen")]
        public string AccountNumber { get; set; } = string.Empty;

        [Display(Name = "Cuenta origen")]
        public string AccountMasked { get; set; } = string.Empty;

        [Display(Name = "Titular")]
        public string HolderFullName { get; set; } = string.Empty;

        [Display(Name = "Monto a retirar")]
        public decimal Amount { get; set; }

        [Display(Name = "Balance actual")]
        public decimal CurrentBalance { get; set; }
    }
}
