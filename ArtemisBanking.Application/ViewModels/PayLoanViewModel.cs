using System.ComponentModel.DataAnnotations;
using ArtemisBanking.Application.Dtos.Loan;
using ArtemisBanking.Application.Dtos.SavingsAccount;

namespace ArtemisBanking.Application.ViewModels
{
    public class PayLoanViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un préstamo")]
        [Display(Name = "Préstamo")]
        public int LoanId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen")]
        [Display(Name = "Cuenta de origen")]
        public string CuentaOrigen { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a pagar")]
        public decimal Monto { get; set; }

        public List<LoanDTO> PrestamosDisponibles { get; set; }
        public List<SavingsAccountDTO> CuentasDisponibles { get; set; }
    }
}