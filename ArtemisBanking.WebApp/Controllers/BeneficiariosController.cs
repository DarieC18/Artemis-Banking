using ArtemisBanking.Application.Dtos.Beneficiary;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBanking.WebApp.Controllers
{
    public class BeneficiariosController : Controller
    {
        private readonly IBeneficiaryService _beneficiaryService;

        public BeneficiariosController(IBeneficiaryService beneficiaryService)
        {
            _beneficiaryService = beneficiaryService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            List<BeneficiaryDTO> beneficiarios =
                await _beneficiaryService.GetBeneficiariesAsync(userId);

            return View(beneficiarios);
        }
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new AddBeneficiaryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddBeneficiaryViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _beneficiaryService.AddBeneficiaryAsync(userId, model);
                TempData["SuccessMessage"] = "Beneficiario agregado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await _beneficiaryService.DeleteBeneficiaryAsync(userId, id);
                TempData["SuccessMessage"] = "Beneficiario eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
