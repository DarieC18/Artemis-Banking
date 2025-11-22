using ArtemisBanking.Application.DTOs.Account;
using ArtemisBanking.WebApp.Models;
using AutoMapper;

namespace ArtemisBanking.WebApp.Mappings
{
    public class AccountWebMappingProfile : Profile
    {
        public AccountWebMappingProfile()
        {
            CreateMap<LoginViewModel, LoginDTO>();
            CreateMap<ForgotPasswordViewModel, ForgotPasswordDTO>();
            CreateMap<ResetPasswordViewModel, ResetPasswordDTO>();
        }
    }
}
