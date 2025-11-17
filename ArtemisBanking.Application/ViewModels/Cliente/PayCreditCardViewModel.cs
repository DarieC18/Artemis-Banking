using System.ComponentModel.DataAnnotations;
using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Application.Dtos.SavingsAccount;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class PayCreditCardViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar una tarjeta de crédito")]
        [Display(Name = "Tarjeta de crédito")]
        public int CreditCardId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen")]
        [Display(Name = "Cuenta de origen")]
        public string CuentaOrigen { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a pagar")]
        public decimal Monto { get; set; }

        public List<CreditCardDTO> TarjetasDisponibles { get; set; }
        public List<SavingsAccountDTO> CuentasDisponibles { get; set; }
    }
}