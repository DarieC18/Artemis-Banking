using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Application.ViewModels;
using ArtemisBanking.Application.Dtos.AdminUsers;
using AutoMapper;
using System.Security.Claims;

namespace ArtemisBanking.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminUsersController : Controller
    {
        private readonly IAdminUserService _adminUserService;
        private readonly IMapper _mapper;

        public AdminUsersController(IAdminUserService adminUserService, IMapper mapper)
        {
            _adminUserService = adminUserService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string? roleFilter = null, CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            const int pageSize = 20;

            var result = await _adminUserService.GetUsersAsync(page, pageSize, roleFilter, cancellationToken);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.RoleFilter = roleFilter;

            var viewModels = _mapper.Map<List<AdminUserListItemViewModel>>(result.Items);
            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateAdminUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAdminUserViewModel model, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = _mapper.Map<CreateAdminUserDTO>(model);
            var result = await _adminUserService.CreateUserAsync(dto, cancellationToken);

            if (result.IsFailure)
            {
                if (result.Errors != null && result.Errors.Any())
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                else
                {
                    ModelState.AddModelError("", result.GeneralError ?? "Error al crear el usuario.");
                }
                return View(model);
            }

            TempData["SuccessMessage"] = "Usuario creado exitosamente. Se ha enviado un correo de activación.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
        {
            var user = await _adminUserService.GetUserByIdAsync(id, cancellationToken);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = _mapper.Map<UpdateAdminUserViewModel>(user);
            ViewBag.IsCliente = user.Role == "Cliente";
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateAdminUserViewModel model, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = _mapper.Map<UpdateAdminUserDTO>(model);
            var result = await _adminUserService.UpdateUserAsync(dto, cancellationToken);

            if (result.IsFailure)
            {
                if (result.Errors != null && result.Errors.Any())
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                else
                {
                    ModelState.AddModelError("", result.GeneralError ?? "Error al actualizar el usuario.");
                }
                return View(model);
            }

            TempData["SuccessMessage"] = "Usuario actualizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id, bool activate, CancellationToken cancellationToken = default)
        {
            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentAdminId))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al administrador actual.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _adminUserService.ToggleUserStatusAsync(id, activate, currentAdminId, cancellationToken);

            if (result.IsFailure)
            {
                TempData["ErrorMessage"] = result.GeneralError ?? "Error al cambiar el estado del usuario.";
            }
            else
            {
                var action = activate ? "activado" : "inactivado";
                TempData["SuccessMessage"] = $"Usuario {action} exitosamente.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

