using System.Linq;
using AutoMapper;
using ArtemisBanking.Application.Dtos.AdminUsers;
using ArtemisBanking.Application.Dtos.Identity;
using ArtemisBanking.Application.ViewModels;

namespace ArtemisBanking.Application.Mappings.DtosAndViewModels
{
    public class AdminUserProfile : Profile
    {
        public AdminUserProfile()
        {
            CreateMap<IdentityUserDto, AdminUserListItemDTO>()
                .ForMember(dest => dest.Role,
                    opt => opt.MapFrom(src => src.Roles.FirstOrDefault() ?? string.Empty));

            CreateMap<AdminUserListItemDTO, AdminUserListItemViewModel>();

            CreateMap<CreateAdminUserViewModel, CreateAdminUserDTO>().ReverseMap();

            CreateMap<UpdateAdminUserViewModel, UpdateAdminUserDTO>().ReverseMap();

            CreateMap<AdminUserListItemDTO, UpdateAdminUserViewModel>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore())
                .ForMember(dest => dest.MontoAdicional, opt => opt.Ignore());
        }
    }
}
