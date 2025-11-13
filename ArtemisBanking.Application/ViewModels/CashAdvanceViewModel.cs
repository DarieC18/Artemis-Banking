using System.ComponentModel.DataAnnotations;
using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Application.Dtos.SavingsAccount;

namespace ArtemisBanking.Application.ViewModels
{
    public class CashAdvanceViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar una tarjeta de crédito")]
        [Display(Name = "Tarjeta de crédito origen")]
        public int CreditCardId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta destino")]
        [Display(Name = "Cuenta de ahorro destino")]
        public string CuentaDestino { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto del avance")]
        public decimal Monto { get; set; }

        public List<CreditCardDTO> TarjetasDisponibles { get; set; }
        public List<SavingsAccountDTO> CuentasDisponibles { get; set; }
    }
}