using AutoMapper;
using ArtemisBanking.Application.Dtos.Identity;
using ArtemisBanking.Application.DTOs.Account;
using ArtemisBanking.Infraestructure.Identity.Entities;

namespace ArtemisBanking.Infraestructure.Identity.Mappings
{
    public class AppUserMappingProfile : Profile
    {
        public AppUserMappingProfile()
        {
            CreateMap<AppUser, IdentityUserDto>();
            CreateMap<AppUser, UserDTO>()
                .ForMember(d => d.Roles, opt => opt.Ignore());

            CreateMap<CreateIdentityUserCommand, AppUser>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.EmailConfirmed, opt => opt.MapFrom(s => s.EmailConfirmed))
                .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive))
                .ForMember(d => d.FechaCreacion, opt => opt.MapFrom(s => s.FechaCreacionUtc))
                .ForMember(d => d.ResetPasswordToken, opt => opt.Ignore())
                .ForMember(d => d.ResetPasswordTokenExpiry, opt => opt.Ignore());

            CreateMap<UpdateIdentityUserCommand, AppUser>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.IsActive, opt => opt.Ignore())
                .ForMember(d => d.FechaCreacion, opt => opt.Ignore())
                .ForMember(d => d.EmailConfirmed, opt => opt.Ignore())
                .ForMember(d => d.ResetPasswordToken, opt => opt.Ignore())
                .ForMember(d => d.ResetPasswordTokenExpiry, opt => opt.Ignore());
        }
    }
}
