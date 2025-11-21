using ArtemisBanking.Application.Dtos.AdminDashboard;
using ArtemisBanking.Application.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Api.Controllers.v1.Admin
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Administrador")]
    public class AdminController : BaseApiController
    {
        private readonly IAdminDashboardService _dashboardService;

        public AdminController(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("dashboard/summary")]
        public async Task<ActionResult<AdminDashboardSummaryDTO>> GetDashboardSummary(CancellationToken cancellationToken = default)
        {
            var summary = await _dashboardService.GetSummaryAsync(cancellationToken);
            return Ok(summary);
        }
    }
}

