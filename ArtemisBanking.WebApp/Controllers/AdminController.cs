using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly IAdminDashboardService _dashboardService;
        private readonly IMapper _mapper;

        public AdminController(IAdminDashboardService dashboardService, IMapper mapper)
        {
            _dashboardService = dashboardService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var summary = await _dashboardService.GetSummaryAsync();
            var viewModel = _mapper.Map<AdminDashboardViewModel>(summary);
            return View(viewModel);
        }
    }
}
