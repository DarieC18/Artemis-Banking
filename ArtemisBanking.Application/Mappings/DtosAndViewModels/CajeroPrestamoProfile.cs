using ArtemisBanking.Application.Dtos.Transaction;
using ArtemisBanking.Application.ViewModels.Cajero;
using AutoMapper;

namespace ArtemisBanking.Application.Mapping.DtosAndViewModels
{
    public class CajeroPrestamoProfile : Profile
    {
        public CajeroPrestamoProfile()
        {
            CreateMap<PagoPrestamoFormViewModel, PayLoanDTO>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            CreateMap<PayLoanPreviewDTO, PagoPrestamoConfirmViewModel>()
                .ForMember(dest => dest.CuentaOrigen,
                    opt => opt.MapFrom(src => src.SourceAccountNumber))
                .ForMember(dest => dest.CuentaOrigenEnmascarada,
                    opt => opt.MapFrom(src => src.SourceAccountMasked))
                .ForMember(dest => dest.BalanceActualCuentaOrigen,
                    opt => opt.MapFrom(src => src.SourceCurrentBalance))
                .ForMember(dest => dest.NumeroPrestamo,
                    opt => opt.MapFrom(src => src.LoanNumber))
                .ForMember(dest => dest.TitularPrestamo,
                    opt => opt.MapFrom(src => src.LoanHolderFullName))
                .ForMember(dest => dest.DeudaPendienteActual,
                    opt => opt.MapFrom(src => src.TotalDebtRemaining))
                .ForMember(dest => dest.CuotasAfectadas,
                    opt => opt.MapFrom(src => src.InstallmentsToAffect))
                .ForMember(dest => dest.LoanId, opt => opt.Ignore())
                .ForMember(dest => dest.MontoSolicitado, opt => opt.Ignore())
                .ForMember(dest => dest.ErrorMessage, opt => opt.Ignore());
        }
    }
}
