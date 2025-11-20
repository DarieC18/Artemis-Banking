using ArtemisBanking.Application.DTOs.Account;
using ArtemisBanking.Infraestructure.Identity.Entities;
using AutoMapper;

namespace ArtemisBanking.Infraestructure.Identity.Mappings
{
    public class AppUserMappingProfile : Profile
    {
        public AppUserMappingProfile()
        {
            CreateMap<AppUser, UserDTO>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore());
        }
    }
}
