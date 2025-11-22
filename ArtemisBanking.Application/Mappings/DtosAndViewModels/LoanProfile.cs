using ArtemisBanking.Application.Dtos.Loan;
using ArtemisBanking.Application.ViewModels;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings.DtosAndViewModels
{
    public class LoanProfile : Profile
    {
        public LoanProfile()
        {
            CreateMap<LoanListItemDTO, LoanListItemViewModel>().ReverseMap();
            CreateMap<ClientForLoanDTO, ClientForLoanViewModel>().ReverseMap();
            CreateMap<AssignLoanDTO, AssignLoanViewModel>().ReverseMap();
            CreateMap<UpdateLoanDTO, UpdateLoanViewModel>().ReverseMap();
            
            // Mapeos para API
            CreateMap<AssignLoanRequestDTO, AssignLoanDTO>();
        }
    }
}
