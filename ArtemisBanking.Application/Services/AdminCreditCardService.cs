using System;
using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Application.DTOs.Email;
using ArtemisBanking.Application.Helpers;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Interfaces.Identity;
using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Services
{
    public class AdminCreditCardService : IAdminCreditCardService
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ICreditCardConsumptionRepository _consumptionRepository;
        private readonly IIdentityUserManager _identityUserManager;
        private readonly ILoanRepository _loanRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public AdminCreditCardService(
            ICreditCardRepository creditCardRepository,
            ICreditCardConsumptionRepository consumptionRepository,
            IIdentityUserManager identityUserManager,
            ILoanRepository loanRepository,
            IEmailService emailService,
            IMapper mapper)
        {
            _creditCardRepository = creditCardRepository;
            _consumptionRepository = consumptionRepository;
            _identityUserManager = identityUserManager;
            _loanRepository = loanRepository;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<CreditCardListItemDTO>> GetCreditCardsAsync(
            int pageNumber,
            int pageSize,
            string? estadoFilter = null,
            string? cedulaFilter = null,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 20;

            List<CreditCard> creditCards;

            if (!string.IsNullOrWhiteSpace(cedulaFilter))
            {
                var allUsers = await _identityUserManager.GetAllAsync(cancellationToken);
                var user = allUsers.FirstOrDefault(u => u.Cedula == cedulaFilter);
                if (user == null)
                {
                    return new PaginatedResult<CreditCardListItemDTO>(new List<CreditCardListItemDTO>(), pageNumber, pageSize, 0);
                }

                // Obtiene todas las tarjetas del usuario
                creditCards = await _creditCardRepository.GetByCedulaAsync(cedulaFilter);
                
                // Filtra por UserId
                creditCards = creditCards.Where(c => c.UserId == user.Id).ToList();

                // Aplica los filtro de estado si existe
                if (!string.IsNullOrWhiteSpace(estadoFilter))
                {
                    if (estadoFilter == "ACTIVA")
                    {
                        creditCards = creditCards.Where(c => c.IsActive).ToList();
                    }
                    else if (estadoFilter == "CANCELADA")
                    {
                        creditCards = creditCards.Where(c => !c.IsActive).ToList();
                    }
                }

                // Ordenar: primero ACTIVAS, luego CANCELADAS, ambas por fecha descendente
                creditCards = creditCards
                    .OrderByDescending(c => c.IsActive)
                    .ThenByDescending(c => c.FechaCreacion)
                    .ToList();
            }
            else
            {
                // Si no hay filtro de cedula, usar estadoFilter directamente
                var estadoParaFiltro = estadoFilter;
                if (string.IsNullOrWhiteSpace(estadoFilter))
                {
                    estadoParaFiltro = "ACTIVA"; // Por defecto mostrar solo activas
                }

                creditCards = await _creditCardRepository.GetAllAsync(estadoParaFiltro, null);
            }

            var totalCount = creditCards.Count;
            var pagedCards = creditCards
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtoList = new List<CreditCardListItemDTO>();
            foreach (var card in pagedCards)
            {
                var user = await _identityUserManager.GetByIdAsync(card.UserId, cancellationToken);

                dtoList.Add(new CreditCardListItemDTO
                {
                    Id = card.Id,
                    NumeroTarjeta = card.NumeroTarjeta,
                    ClienteNombre = user?.Nombre ?? "",
                    ClienteApellido = user?.Apellido ?? "",
                    LimiteCredito = card.LimiteCredito,
                    FechaExpiracion = card.FechaExpiracion, // Ya viene como string MM/yy
                    DeudaActual = card.DeudaActual,
                    Estado = card.IsActive ? "ACTIVA" : "CANCELADA"
                });
            }

            return new PaginatedResult<CreditCardListItemDTO>(dtoList, pageNumber, pageSize, totalCount);
        }

        public async Task<CreditCardDetailDTO?> GetCreditCardByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var card = await _creditCardRepository.GetByIdAsync(id);
            if (card == null)
                return null;

            var consumos = card.Consumos
                .OrderByDescending(c => c.FechaConsumo)
                .ToList();

            return new CreditCardDetailDTO
            {
                NumeroTarjeta = card.NumeroTarjeta,
                Ultimos4Digitos = card.NumeroTarjeta.Length >= 4 
                    ? card.NumeroTarjeta.Substring(card.NumeroTarjeta.Length - 4) 
                    : card.NumeroTarjeta,
                LimiteCredito = card.LimiteCredito,
                DeudaActual = card.DeudaActual,
                CreditoDisponible = card.LimiteCredito - card.DeudaActual,
                FechaExpiracion = card.FechaExpiracion,
                FechaCreacion = card.FechaCreacion,
                Consumos = _mapper.Map<List<CreditCardConsumptionDTO>>(consumos)
            };
        }

        public async Task<List<ClientForCreditCardDTO>> GetEligibleClientsAsync(CancellationToken cancellationToken = default)
        {
            var allUsers = await _identityUserManager.GetAllAsync(cancellationToken);
            var clientes = allUsers
                .Where(u => u.Roles.Any(r => r.Equals("Cliente", StringComparison.OrdinalIgnoreCase)) && u.IsActive)
                .ToList();

            var eligibleClients = new List<ClientForCreditCardDTO>();

            foreach (var cliente in clientes)
            {
                // Calcula la deuda total: prestamos + tarjetas
                var loans = await _loanRepository.GetByUserIdAsync(cliente.Id);
                var deudaPrestamos = loans
                    .Where(l => l.IsActive)
                    .Sum(l => l.MontoPendiente);

                var creditCards = await _creditCardRepository.GetActiveByUserIdAsync(cliente.Id);
                var deudaTarjetas = creditCards.Sum(c => c.DeudaActual);

                var deudaTotal = deudaPrestamos + deudaTarjetas;

                eligibleClients.Add(new ClientForCreditCardDTO
                {
                    UserId = cliente.Id,
                    Cedula = cliente.Cedula,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Email = cliente.Email,
                    DeudaTotal = deudaTotal
                });
            }

            return eligibleClients.OrderBy(c => c.Nombre).ToList();
        }

        public async Task<decimal> GetAverageDebtAsync(CancellationToken cancellationToken = default)
        {
            return await _creditCardRepository.GetAverageDebtAsync();
        }

        public async Task<Result> AssignCreditCardAsync(AssignCreditCardDTO request, string adminUserId, CancellationToken cancellationToken = default)
        {
            if (request.LimiteCredito <= 0)
            {
                return Result.Fail("El límite de crédito es obligatorio y debe ser mayor a cero");
            }

            var user = await _identityUserManager.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Fail("Cliente no encontrado");
            }

            // Genera el numero unico de 16 dígitos
            string numeroTarjeta;
            do
            {
                numeroTarjeta = GenerateCardNumber();
            } while (await _creditCardRepository.ExistsByNumberAsync(numeroTarjeta));

            // Calcula la fecha de expiracion (+3 años) y la convierte a formato MM/yy
            var fechaExpiracionDateTime = DateTime.Now.AddYears(3);
            var fechaExpiracionString = fechaExpiracionDateTime.ToString("MM/yy");

            // Genera el CVC aleatorio de 3 digitos y cifra con SHA-256
            var cvcCifrado = CvcEncryptionHelper.GenerateAndEncryptCVC();

            // Crea la tarjeta
            var nuevaTarjeta = new CreditCard
            {
                NumeroTarjeta = numeroTarjeta,
                LimiteCredito = request.LimiteCredito,
                DeudaActual = 0,
                FechaExpiracion = fechaExpiracionString,
                CVCHash = cvcCifrado,
                IsActive = true,
                FechaCreacion = DateTime.Now,
                UserId = request.UserId,
                AdminUserId = adminUserId,
                Consumos = new List<CreditCardConsumption>()
            };

            await _creditCardRepository.AddAsync(nuevaTarjeta);

            return Result.Ok();
        }

        public async Task<Result> UpdateCreditCardLimitAsync(UpdateCreditCardLimitDTO request, CancellationToken cancellationToken = default)
        {
            if (request.LimiteCredito <= 0)
            {
                return Result.Fail("El límite de crédito debe ser mayor a cero");
            }

            var tarjeta = await _creditCardRepository.GetByIdAsync(request.Id);
            if (tarjeta == null)
            {
                return Result.Fail("Tarjeta no encontrada");
            }

            if (!tarjeta.IsActive)
            {
                return Result.Fail("No se puede actualizar el límite de una tarjeta cancelada");
            }

            // Valida que el nuevo limite no sea menor que la deuda actual
            if (request.LimiteCredito < tarjeta.DeudaActual)
            {
                return Result.Fail($"El nuevo límite (RD${request.LimiteCredito}) no puede ser menor " +
                                 $"que la deuda actual (RD${tarjeta.DeudaActual})");
            }

            var limiteAnterior = tarjeta.LimiteCredito;
            tarjeta.LimiteCredito = request.LimiteCredito;
            await _creditCardRepository.UpdateAsync(tarjeta);

            // Envia el correo al cliente
            var user = await _identityUserManager.GetByIdAsync(tarjeta.UserId);
            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                var ultimos4Digitos = tarjeta.NumeroTarjeta.Length >= 4 
                    ? tarjeta.NumeroTarjeta.Substring(tarjeta.NumeroTarjeta.Length - 4) 
                    : tarjeta.NumeroTarjeta;

                var asunto = $"Límite de tarjeta [{ultimos4Digitos}] modificado";
                var cuerpo = $@"
                Estimado {user.Nombre} {user.Apellido},

                El límite de su tarjeta de crédito terminada en {ultimos4Digitos} ha sido modificado.

                Nuevo límite aprobado: RD${request.LimiteCredito}

                Si tiene alguna pregunta, contáctenos.

                Gracias,
                Artemis Banking";

                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = asunto,
                    Body = cuerpo
                });
            }

            return Result.Ok();
        }

        public async Task<Result> CancelCreditCardAsync(int id, CancellationToken cancellationToken = default)
        {
            var tarjeta = await _creditCardRepository.GetByIdAsync(id);
            if (tarjeta == null)
            {
                return Result.Fail("Tarjeta no encontrada");
            }

            if (!tarjeta.IsActive)
            {
                return Result.Fail("La tarjeta ya está cancelada");
            }

            // Valida que la deuda sea cero
            if (tarjeta.DeudaActual > 0)
            {
                return Result.Fail("Para cancelar esta tarjeta, el cliente debe " +
                                 "saldar la totalidad de la deuda pendiente");
            }

            tarjeta.IsActive = false;
            await _creditCardRepository.UpdateAsync(tarjeta);

            return Result.Ok();
        }

        private string GenerateCardNumber()
        {
            var random = new Random();
            var numero = "";
            for (int i = 0; i < 16; i++)
            {
                numero += random.Next(0, 10).ToString();
            }
            return numero;
        }

    }
}
