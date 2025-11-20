using AutoMapper;
using ArtemisBanking.Application.DTOs.Account;
using ArtemisBanking.Application.ViewModels;

namespace ArtemisBanking.Application.Mappings.DtosAndViewModels
{
    public class AccountMappingProfile : Profile
    {
        public AccountMappingProfile()
        {
            CreateMap<LoginDTO, LoginViewModel>().ReverseMap();
            CreateMap<ForgotPasswordDTO, ForgotPasswordViewModel>().ReverseMap();
            CreateMap<ResetPasswordDTO, ResetPasswordViewModel>().ReverseMap();
        }
    }
}
