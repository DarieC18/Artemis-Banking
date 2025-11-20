using ArtemisBanking.Application.Dtos.CreditCard;
using ArtemisBanking.Application.ViewModels;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings.DtosAndViewModels
{
    public class CreditCardProfile : Profile
    {
        public CreditCardProfile()
        {
            CreateMap<CreditCardListItemDTO, CreditCardListItemViewModel>();

            CreateMap<ClientForCreditCardDTO, ClientForCreditCardViewModel>();

            CreateMap<AssignCreditCardViewModel, AssignCreditCardDTO>();

            CreateMap<UpdateCreditCardLimitViewModel, UpdateCreditCardLimitDTO>();
            
            // Mapeos para API
            CreateMap<AssignCreditCardRequestDTO, AssignCreditCardDTO>();
        }
    }
}
