using ArtemisBanking.Application.Dtos.SavingsAccount;
using ArtemisBanking.Application.Dtos.Transaction;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings.API
{
    public class SavingsAccountApiProfile : Profile
    {
        public SavingsAccountApiProfile()
        {
            // Mapeo de SavingsAccountListItemDTO a SavingsAccountApiListItemDTO
            CreateMap<SavingsAccountListItemDTO, SavingsAccountApiListItemDTO>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.Tipo, opt => opt.MapFrom(s => s.EsPrincipal ? "principal" : "secundaria"));

            // Mapeo de PaginatedResult a SavingsAccountApiListResponseDTO
            CreateMap<SavingsAccountListItemDTO, SavingsAccountApiListItemDTO>();

            // Mapeo de SavingsAccountApiCreateRequestDTO a AssignSavingsAccountDTO
            CreateMap<SavingsAccountApiCreateRequestDTO, AssignSavingsAccountDTO>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.ClienteId))
                .ForMember(d => d.BalanceInicial, opt => opt.MapFrom(s => s.BalanceInicial));

            // Mapeo de SavingsAccountDetailDTO a SavingsAccountApiTransactionsResponseDTO
            CreateMap<SavingsAccountDetailDTO, SavingsAccountApiTransactionsResponseDTO>()
                .ForMember(d => d.Transacciones, opt => opt.MapFrom(s => s.Transacciones));

            // Mapeo de TransactionDTO a SavingsAccountApiTransactionDTO
            CreateMap<TransactionDTO, SavingsAccountApiTransactionDTO>()
                .ForMember(d => d.Fecha, opt => opt.MapFrom(s => s.FechaTransaccion))
                .ForMember(d => d.Tipo, opt => opt.MapFrom(s =>
                    s.Tipo.Contains("CRÉDITO") || s.Tipo.Contains("CREDITO") || s.Tipo == "CREDITO" || s.Tipo == "DEPÓSITO"
                        ? "CRÉDITO" : "DÉBITO"));
        }
    }
}

