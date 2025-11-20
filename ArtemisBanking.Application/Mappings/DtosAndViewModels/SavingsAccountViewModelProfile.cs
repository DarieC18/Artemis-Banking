using ArtemisBanking.Application.Dtos.SavingsAccount;
using ArtemisBanking.Application.ViewModels;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings.DtosAndViewModels
{
    public class SavingsAccountViewModelProfile : Profile
    {
        public SavingsAccountViewModelProfile()
        {
            CreateMap<SavingsAccountListItemDTO, SavingsAccountListItemViewModel>();
            CreateMap<ClientForSavingsAccountDTO, ClientForSavingsAccountViewModel>();
            CreateMap<AssignSavingsAccountViewModel, AssignSavingsAccountDTO>();
        }
    }
}

