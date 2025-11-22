using ArtemisBanking.Application.Dtos.Loan;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings.EntitiesAndDtos
{
    public class LoanProfile : Profile
    {
        public LoanProfile()
        {
            CreateMap<Loan, LoanDTO>();

            CreateMap<LoanPaymentSchedule, LoanPaymentScheduleDTO>();

            CreateMap<Loan, LoanDetailDTO>();
        }
    }
}
