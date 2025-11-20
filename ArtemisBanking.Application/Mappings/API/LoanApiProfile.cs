using ArtemisBanking.Application.Dtos.Loan;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings.DtosAndViewModels
{
    public class LoanApiProfile : Profile
    {
        public LoanApiProfile()
        {
            // Mapeo de LoanListItemDTO a LoanApiListItemDTO
            CreateMap<LoanListItemDTO, LoanApiListItemDTO>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.NumeroIdentificador, opt => opt.MapFrom(s => s.NumeroPrestamo))
                .ForMember(d => d.CantidadTotalCuotas, opt => opt.MapFrom(s => s.CuotasTotales))
                .ForMember(d => d.EnMora, opt => opt.MapFrom(s => s.EstadoPago == "En mora"));

            // Mapeo de LoanListResponseDTO a LoanApiListResponseDTO
            CreateMap<LoanListResponseDTO, LoanApiListResponseDTO>()
                .ForMember(d => d.Data, opt => opt.MapFrom(s => s.Items))
                .ForMember(d => d.CurrentPage, opt => opt.MapFrom(s => s.PageNumber))
                .ForMember(d => d.TotalRecords, opt => opt.MapFrom(s => s.TotalCount));

            // Mapeo de LoanApiCreateRequestDTO a AssignLoanDTO
            CreateMap<LoanApiCreateRequestDTO, AssignLoanDTO>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.ClienteId))
                .ForMember(d => d.MontoCapital, opt => opt.MapFrom(s => s.MontoPrestar))
                .ForMember(d => d.TasaInteres, opt => opt.MapFrom(s => s.TasaInteresAnual));

            // Mapeo de LoanDetailDTO a LoanApiDetailResponseDTO
            CreateMap<LoanDetailDTO, LoanApiDetailResponseDTO>()
                .ForMember(d => d.PrestamoId, opt => opt.MapFrom(s => s.NumeroPrestamo))
                .ForMember(d => d.NumeroIdentificador, opt => opt.MapFrom(s => s.NumeroPrestamo))
                .ForMember(d => d.TablaAmortizacion, opt => opt.MapFrom(s => s.TablaAmortizacion));

            // Mapeo de LoanPaymentScheduleDTO a LoanApiPaymentScheduleDTO
            CreateMap<LoanPaymentScheduleDTO, LoanApiPaymentScheduleDTO>()
                .ForMember(d => d.CuotaNumero, opt => opt.MapFrom(s => s.NumeroCuota))
                .ForMember(d => d.ValorCuota, opt => opt.MapFrom(s => s.ValorCuota))
                .ForMember(d => d.EstadoPago, opt => opt.MapFrom(s => s.Pagada));

            // Mapeo de LoanApiUpdateRateRequestDTO a UpdateLoanDTO
            CreateMap<LoanApiUpdateRateRequestDTO, UpdateLoanDTO>()
                .ForMember(d => d.TasaInteres, opt => opt.MapFrom(s => s.NuevaTasaInteres))
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.MontoCapital, opt => opt.Ignore())
                .ForMember(d => d.PlazoMeses, opt => opt.Ignore());
        }
    }
}

