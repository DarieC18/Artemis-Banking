using System.ComponentModel.DataAnnotations;
using ArtemisBanking.Application.Dtos.SavingsAccount;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class TransferBetweenAccountsViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen")]
        [Display(Name = "Cuenta de origen")]
        public string CuentaOrigen { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta de destino")]
        [Display(Name = "Cuenta de destino")]
        public string CuentaDestino { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a transferir")]
        public decimal Monto { get; set; }

        public List<SavingsAccountDTO> CuentasDisponibles { get; set; }
    }
}