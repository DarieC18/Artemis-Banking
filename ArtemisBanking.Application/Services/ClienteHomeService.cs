using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Application.Dtos.Loan;
using ArtemisBanking.Application.Dtos.SavingsAccount;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels.Cliente;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Services
{
    public class ClienteHomeService : IClienteHomeService
    {
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly IMapper _mapper;
        private readonly ILoanPaymentScheduleRepository _scheduleRepository;

        public ClienteHomeService(
            ISavingsAccountRepository savingsAccountRepository,
            ILoanRepository loanRepository,
            ICreditCardRepository creditCardRepository,
            IMapper mapper,
            ILoanPaymentScheduleRepository scheduleRepository)
        {
            _savingsAccountRepository = savingsAccountRepository;
            _loanRepository = loanRepository;
            _creditCardRepository = creditCardRepository;
            _mapper = mapper;
            _scheduleRepository = scheduleRepository;
        }

        public async Task<HomeViewModel> GetHomeDataAsync(string userId)
        {
            var cuentas = await _savingsAccountRepository.GetByUserIdAsync(userId);
            var prestamos = await _loanRepository.GetActiveByUserIdAsync(userId);
            var tarjetas = await _creditCardRepository.GetActiveByUserIdAsync(userId);

            foreach (var prestamo in prestamos)
            {
                var cuotas = await _scheduleRepository.GetByLoanIdAsync(prestamo.Id);
                prestamo.TablaAmortizacion = cuotas;

                prestamo.EstadoPago = RecalcularEstadoPrestamo(cuotas);
            }

            return new HomeViewModel
            {
                CuentasDeAhorro = _mapper.Map<List<SavingsAccountDTO>>(cuentas),
                Prestamos = _mapper.Map<List<LoanDTO>>(prestamos),
                TarjetasDeCredito = _mapper.Map<List<CreditCardDTO>>(tarjetas)
            };
        }
        private string RecalcularEstadoPrestamo(List<LoanPaymentSchedule> cuotas)
        {
            var enMora = cuotas.Any(c => c.Atrasada == true);

            return enMora ? "En mora" : "Al dia";
        }
    }
}
