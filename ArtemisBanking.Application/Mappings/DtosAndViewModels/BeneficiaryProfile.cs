using ArtemisBanking.Application.Dtos.Beneficiary;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings.DtosAndViewModels
{
    public class BeneficiaryProfile : Profile
    {
        public BeneficiaryProfile()
        {
            CreateMap<Beneficiary, BeneficiaryDTO>()
                .ForMember(d => d.NombreCompleto,
                    opt => opt.MapFrom(s =>
                        (s.NombreBeneficiario + " " + s.ApellidoBeneficiario).Trim()));
        }
    }
}
