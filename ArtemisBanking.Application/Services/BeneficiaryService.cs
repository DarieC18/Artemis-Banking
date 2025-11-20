using ArtemisBanking.Application.Dtos.Beneficiary;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels.Cliente;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Services
{
    public class BeneficiaryService : IBeneficiaryService
    {
        private readonly IBeneficiaryRepository _beneficiaryRepository;
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly IMapper _mapper;

        public BeneficiaryService(
            IBeneficiaryRepository beneficiaryRepository,
            ISavingsAccountRepository savingsAccountRepository,
            IMapper mapper,
            IUserInfoService userInfoService)
        {
            _beneficiaryRepository = beneficiaryRepository;
            _savingsAccountRepository = savingsAccountRepository;
            _mapper = mapper;
            _userInfoService = userInfoService;
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

            var userInfo = await _userInfoService.GetUserBasicInfoByIdAsync(cuenta.UserId);
            var nombre = userInfo?.Nombre?.Trim() ?? string.Empty;
            var apellido = userInfo?.Apellido?.Trim() ?? string.Empty;

            var nuevo = new Beneficiary
            {
                UserId = userId,
                NumeroCuentaBeneficiario = model.NumeroCuentaBeneficiario,
                NombreBeneficiario = nombre,
                ApellidoBeneficiario = apellido,
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
