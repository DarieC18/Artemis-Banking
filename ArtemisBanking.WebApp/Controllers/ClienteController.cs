using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBanking.WebApp.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClienteController : Controller
    {
        private readonly IClienteHomeService _clienteHomeService;

        public ClienteController(IClienteHomeService clienteHomeService)
        {
            _clienteHomeService = clienteHomeService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
        {
                return RedirectToAction("Account");
            }

            HomeViewModel model = await _clienteHomeService.GetHomeDataAsync(userId);

            return View(model);
        }
    }
}
