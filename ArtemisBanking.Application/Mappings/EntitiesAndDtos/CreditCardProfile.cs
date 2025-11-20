using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings.EntitiesAndDtos
{
    public class CreditCardProfile : Profile
    {
        public CreditCardProfile()
        {
            CreateMap<CreditCard, CreditCardDTO>()
                .ForMember(d => d.Ultimos4Digitos,
                    opt => opt.MapFrom(s => s.NumeroTarjeta.Length >= 4
                        ? s.NumeroTarjeta.Substring(s.NumeroTarjeta.Length - 4)
                        : s.NumeroTarjeta))
                .ForMember(d => d.CreditoDisponible,
                    opt => opt.MapFrom(s => s.LimiteCredito - s.DeudaActual));

            CreateMap<CreditCard, CreditCardDetailDTO>()
                .ForMember(d => d.Ultimos4Digitos,
                    opt => opt.MapFrom(s => s.NumeroTarjeta.Length >= 4
                        ? s.NumeroTarjeta.Substring(s.NumeroTarjeta.Length - 4)
                        : s.NumeroTarjeta))
                .ForMember(d => d.CreditoDisponible,
                    opt => opt.MapFrom(s => s.LimiteCredito - s.DeudaActual));

            CreateMap<CreditCardConsumption, CreditCardConsumptionDTO>();
        }
    }
}
