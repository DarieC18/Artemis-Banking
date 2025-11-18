using ArtemisBanking.Application.Dtos.SavingsAccount;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings.EntitiesAndDtos
{
    public class SavingsAccountProfile : Profile
    {
        public SavingsAccountProfile()
        {
            CreateMap<SavingsAccount, SavingsAccountDTO>()
                .ForMember(d => d.NumeroCuenta,
                    opt => opt.MapFrom(s => s.NumeroCuenta))
                .ForMember(d => d.Balance,
                    opt => opt.MapFrom(s => s.Balance))
                .ForMember(d => d.EsPrincipal,
                    opt => opt.MapFrom(s => s.EsPrincipal))
                .ForMember(d => d.TipoCuenta,
                    opt => opt.MapFrom(s => s.EsPrincipal ? "Principal" : "Secundaria"));

            CreateMap<SavingsAccount, SavingsAccountDetailDTO>()
                .ForMember(d => d.NumeroCuenta,
                    opt => opt.MapFrom(s => s.NumeroCuenta))
                .ForMember(d => d.Balance,
                    opt => opt.MapFrom(s => s.Balance))
                .ForMember(d => d.EsPrincipal,
                    opt => opt.MapFrom(s => s.EsPrincipal))
                .ForMember(d => d.TipoCuenta,
                    opt => opt.MapFrom(s => s.EsPrincipal ? "Principal" : "Secundaria"))
                .ForMember(d => d.FechaCreacion,
                    opt => opt.MapFrom(s => s.FechaCreacion))
                // AutoMapper ya sabe usar el TransactionProfile para cada item
                .ForMember(d => d.Transacciones,
                    opt => opt.MapFrom(s => s.Transactions));

            CreateMap<CreateSavingsAccountDTO, SavingsAccount>();
        }
    }
}
