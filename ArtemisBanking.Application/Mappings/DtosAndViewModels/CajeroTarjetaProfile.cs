using ArtemisBanking.Application.Dtos.Transaction;
using ArtemisBanking.Application.ViewModels.Cajero;
using AutoMapper;

namespace ArtemisBanking.Application.Mapping
{
    public class CajeroTarjetaProfile : Profile
    {
        public CajeroTarjetaProfile()
        {
            CreateMap<PagoTarjetaFormViewModel, PayCreditCardDTO>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreditCardId, opt => opt.Ignore());

            CreateMap<PayCreditCardPreviewDTO, PagoTarjetaConfirmViewModel>()
                .ForMember(dest => dest.CuentaOrigen,
                    opt => opt.MapFrom(src => src.SourceAccountNumber))
                .ForMember(dest => dest.CuentaOrigenEnmascarada,
                    opt => opt.MapFrom(src => src.SourceAccountMasked))
                .ForMember(dest => dest.BalanceActualCuentaOrigen,
                    opt => opt.MapFrom(src => src.SourceCurrentBalance))
                .ForMember(dest => dest.NumeroTarjetaEnmascarada,
                    opt => opt.MapFrom(src => src.CardNumberMasked))
                .ForMember(dest => dest.TitularTarjeta,
                    opt => opt.MapFrom(src => src.CardHolderFullName))
                .ForMember(dest => dest.DeudaActual,
                    opt => opt.MapFrom(src => src.CurrentDebt))
                .ForMember(dest => dest.MontoAplicadoReal,
                    opt => opt.MapFrom(src => src.RealPaymentAmount))
                .ForMember(dest => dest.CreditCardId, opt => opt.Ignore())
                .ForMember(dest => dest.MontoSolicitado, opt => opt.Ignore())
                .ForMember(dest => dest.ErrorMessage, opt => opt.Ignore());
        }
    }
}
