using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Application.Dtos.Loan;
using ArtemisBanking.Application.Dtos.SavingsAccount;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels.Cliente;
using AutoMapper;

namespace ArtemisBanking.Application.Services
{
    public class ClienteHomeService : IClienteHomeService
    {
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly IMapper _mapper;

        public ClienteHomeService(
            ISavingsAccountRepository savingsAccountRepository,
            ILoanRepository loanRepository,
            ICreditCardRepository creditCardRepository,
            IMapper mapper)
        {
            _savingsAccountRepository = savingsAccountRepository;
            _loanRepository = loanRepository;
            _creditCardRepository = creditCardRepository;
            _mapper = mapper;
        }

        public async Task<HomeViewModel> GetHomeDataAsync(string userId)
        {
            var cuentas = await _savingsAccountRepository.GetByUserIdAsync(userId);
            var prestamos = await _loanRepository.GetActiveByUserIdAsync(userId);
            var tarjetas = await _creditCardRepository.GetActiveByUserIdAsync(userId);

            return new HomeViewModel
            {
                CuentasDeAhorro = _mapper.Map<List<SavingsAccountDTO>>(cuentas),
                Prestamos = _mapper.Map<List<LoanDTO>>(prestamos),
                TarjetasDeCredito = _mapper.Map<List<CreditCardDTO>>(tarjetas)
            };
        }
    }
}
