using ArtemisBanking.Application.DTOs.Commerce;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.MappingProfiles
{
    public class CommerceProfile : Profile
    {
        public CommerceProfile()
        {
            CreateMap<Commerce, CommerceListDto>()
                .ForMember(d => d.Logo, opt => opt.MapFrom(s => s.LogoUrl));

            CreateMap<Commerce, CommerceDetailDto>()
                .ForMember(d => d.Logo, opt => opt.MapFrom(s => s.LogoUrl));

            CreateMap<CommerceCreateUpdateDto, Commerce>()
                .ForMember(d => d.LogoUrl, opt => opt.MapFrom(s => s.Logo));
        }
    }
}
