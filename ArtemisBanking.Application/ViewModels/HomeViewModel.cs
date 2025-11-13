using ArtemisBanking.Application.Dtos.SavingsAccount;
using ArtemisBanking.Application.Dtos.Loan;
using ArtemisBanking.Application.Dtos.CreditCard;

namespace ArtemisBanking.Application.ViewModels
{
    public class HomeViewModel
    {
        public List<SavingsAccountDTO> CuentasDeAhorro { get; set; }
        public List<LoanDTO> Prestamos { get; set; }
        public List<CreditCardDTO> TarjetasDeCredito { get; set; }
    }
}