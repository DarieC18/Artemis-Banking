using ArtemisBanking.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBanking.WebApp.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClienteController : Controller
    {
        private readonly IClienteHomeService _clienteHomeService;
        private readonly IClienteProductoService _clienteProductoService;

        public ClienteController(IClienteHomeService clienteHomeService, IClienteProductoService clienteProductoService)
        {
            _clienteHomeService = clienteHomeService;
            _clienteProductoService = clienteProductoService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var model = await _clienteHomeService.GetHomeDataAsync(userId);

            return View(model);
        }
        public IActionResult Home()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DetalleCuenta(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var vm = await _clienteProductoService.GetDetalleCuentaAsync(userId, id);

            if (vm == null)
                return RedirectToAction("Index");

            return View(vm);
        }

        public async Task<IActionResult> DetallePrestamo(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var vm = await _clienteProductoService.GetDetallePrestamoAsync(userId, id);

            if (vm == null)
                return RedirectToAction("Index");

            return View(vm);
        }
        public async Task<IActionResult> DetalleTarjeta(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var vm = await _clienteProductoService.GetDetalleTarjetaAsync(userId, id);

            if (vm == null)
                return RedirectToAction("Index");

            return View(vm);
        }
    }
}
