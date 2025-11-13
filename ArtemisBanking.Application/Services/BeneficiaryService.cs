using ArtemisBanking.Application.Dtos.Beneficiary;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Services
{
    public class BeneficiaryService : IBeneficiaryService
    {
        private readonly IBeneficiaryRepository _beneficiaryRepository;
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly IMapper _mapper;

        public BeneficiaryService(
            IBeneficiaryRepository beneficiaryRepository,
            ISavingsAccountRepository savingsAccountRepository,
            IMapper mapper)
        {
            _beneficiaryRepository = beneficiaryRepository;
            _savingsAccountRepository = savingsAccountRepository;
            _mapper = mapper;
        }

        public async Task<List<BeneficiaryDTO>> GetBeneficiariesAsync(string userId)
        {
            var entities = await _beneficiaryRepository.GetByUserIdAsync(userId);
            return _mapper.Map<List<BeneficiaryDTO>>(entities);
        }

        public async Task AddBeneficiaryAsync(string userId, AddBeneficiaryViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NumeroCuentaBeneficiario))
                throw new ArgumentException("El número de cuenta del beneficiario es obligatorio.");

            var cuenta = await _savingsAccountRepository.GetByAccountNumberAsync(model.NumeroCuentaBeneficiario);
            if (cuenta == null || !cuenta.IsActive)
                throw new InvalidOperationException("La cuenta de beneficiario no existe o está inactiva.");

            var existente = await _beneficiaryRepository
                .GetByUserAndAccountAsync(userId, model.NumeroCuentaBeneficiario);

            if (existente != null)
                throw new InvalidOperationException("Este beneficiario ya está registrado.");

            var nuevo = new Beneficiary
            {
                UserId = userId,
                NumeroCuentaBeneficiario = model.NumeroCuentaBeneficiario,
                NombreBeneficiario = string.Empty,
                ApellidoBeneficiario = string.Empty,
                FechaCreacion = DateTime.UtcNow
            };

            await _beneficiaryRepository.AddAsync(nuevo);
        }

        public async Task DeleteBeneficiaryAsync(string userId, int beneficiaryId)
        {
            var entity = await _beneficiaryRepository.GetByIdAsync(beneficiaryId);

            if (entity == null || entity.UserId != userId)
                throw new InvalidOperationException("Beneficiario no encontrado o no pertenece al usuario.");

            await _beneficiaryRepository.DeleteAsync(entity);
        }
    }
}
