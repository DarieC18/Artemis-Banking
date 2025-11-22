using AutoMapper;
using ArtemisBanking.Application.Dtos.AdminDashboard;
using ArtemisBanking.Application.ViewModels;

namespace ArtemisBanking.Application.Mappings.DtosAndViewModels
{
    public class AdminDashboardProfile : Profile
    {
        public AdminDashboardProfile()
        {
            CreateMap<AdminDashboardSummaryDTO, AdminDashboardViewModel>().ReverseMap();
        }
    }
}
