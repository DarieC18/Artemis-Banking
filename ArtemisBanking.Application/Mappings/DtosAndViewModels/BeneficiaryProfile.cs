using ArtemisBanking.Application.Dtos.Beneficiary;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings.DtosAndViewModels
{
    public class BeneficiaryProfile : Profile
    {
        public BeneficiaryProfile()
        {
            CreateMap<Beneficiary, BeneficiaryDTO>();

            CreateMap<CreateBeneficiaryDTO, Beneficiary>()
                .ForMember(d => d.FechaCreacion, opt => opt.Ignore());
        }
    }
}
