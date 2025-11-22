using ArtemisBanking.Application.Dtos.CreditCard;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings.API
{
    public class CreditCardApiProfile : Profile
    {
        public CreditCardApiProfile()
        {
            // Mapeo de CreditCardListItemDTO a CreditCardApiListItemDTO
            CreateMap<CreditCardListItemDTO, CreditCardApiListItemDTO>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.DeudaTotal, opt => opt.MapFrom(s => s.DeudaActual));

            // Mapeo de CreditCardListResponseDTO a CreditCardApiListResponseDTO
            CreateMap<CreditCardListResponseDTO, CreditCardApiListResponseDTO>()
                .ForMember(d => d.Data, opt => opt.MapFrom(s => s.Items))
                .ForMember(d => d.CurrentPage, opt => opt.MapFrom(s => s.PageNumber));

            // Mapeo de CreditCardApiCreateRequestDTO a AssignCreditCardDTO
            CreateMap<CreditCardApiCreateRequestDTO, AssignCreditCardDTO>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.ClienteId));

            // Mapeo de CreditCardDetailDTO a CreditCardApiDetailResponseDTO
            CreateMap<CreditCardDetailDTO, CreditCardApiDetailResponseDTO>()
                .ForMember(d => d.TarjetaId, opt => opt.MapFrom(s => s.NumeroTarjeta))
                .ForMember(d => d.DeudaTotal, opt => opt.MapFrom(s => s.DeudaActual))
                .ForMember(d => d.Consumos, opt => opt.MapFrom(s => s.Consumos));

            // Mapeo de CreditCardConsumptionDTO a CreditCardApiConsumptionDTO
            CreateMap<CreditCardConsumptionDTO, CreditCardApiConsumptionDTO>()
                .ForMember(d => d.Fecha, opt => opt.MapFrom(s => s.FechaConsumo))
                .ForMember(d => d.Comercio, opt => opt.MapFrom(s =>
                    s.Comercio == "Avance de efectivo" ? "AVANCE" : s.Comercio))
                .ForMember(d => d.Estado, opt => opt.MapFrom(s => s.Estado == "APROBADO" ? "APROBADO" : "RECHAZADO"));

            // Mapeo de CreditCardApiUpdateLimitRequestDTO a UpdateCreditCardLimitDTO
            CreateMap<CreditCardApiUpdateLimitRequestDTO, UpdateCreditCardLimitDTO>()
                .ForMember(d => d.LimiteCredito, opt => opt.MapFrom(s => s.NuevoLimite))
                .ForMember(d => d.Id, opt => opt.Ignore());
        }
    }
}

