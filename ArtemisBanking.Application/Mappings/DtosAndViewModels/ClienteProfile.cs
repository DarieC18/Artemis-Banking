using ArtemisBanking.Application.ViewModels.Cliente;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings
{
    public class ClienteProfile : Profile
    {
        public ClienteProfile()
        {
            CreateMap<SavingsAccount, DetalleCuentaViewModel>()
                .ForMember(dest => dest.CuentaId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.NumeroCuenta, opt => opt.MapFrom(src => src.NumeroCuenta))
                .ForMember(dest => dest.BalanceActual, opt => opt.MapFrom(src => src.Balance))
                .ForMember(dest => dest.EsPrincipal, opt => opt.MapFrom(src => src.EsPrincipal))
                .ForMember(dest => dest.Transacciones, opt => opt.Ignore());

            CreateMap<Transaction, TransaccionDetalleViewModel>()
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.FechaTransaccion))
                .ForMember(dest => dest.Monto, opt => opt.MapFrom(src => src.Monto))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado));
        }
    }
}
