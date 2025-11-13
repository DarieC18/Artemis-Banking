using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings.DtosAndViewModels
{
    public class CreditCardProfile : Profile
    {
        public CreditCardProfile()
        {
            CreateMap<CreditCard, CreditCardDTO>();

            CreateMap<CreditCard, CreditCardDetailDTO>();

            CreateMap<CreditCardConsumption, CreditCardConsumptionDTO>();
        }
    }
}
