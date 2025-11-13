using System.ComponentModel.DataAnnotations;
using ArtemisBanking.Application.Dtos.Beneficiary;
using ArtemisBanking.Application.Dtos.SavingsAccount;

namespace ArtemisBanking.Application.ViewModels
{
    public class TransactionBeneficiaryViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un beneficiario")]
        [Display(Name = "Beneficiario")]
        public int BeneficiaryId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a transferir")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen")]
        [Display(Name = "Cuenta de origen")]
        public string CuentaOrigen { get; set; }

        public List<BeneficiaryDTO> BeneficiariosDisponibles { get; set; }
        public List<SavingsAccountDTO> CuentasDisponibles { get; set; }
    }
}