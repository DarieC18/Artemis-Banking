using ArtemisBanking.Application.Dtos.SavingsAccount;
using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class TransactionExpressViewModel
    {
        [Required(ErrorMessage = "El número de cuenta destino es requerido")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de cuenta destino")]
        public string CuentaDestino { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a transferir")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen")]
        [Display(Name = "Cuenta de origen")]
        public string CuentaOrigen { get; set; }

        public List<SavingsAccountDTO> CuentasDisponibles { get; set; }
    }
}