using System.Security.Claims;
using System.Threading.Tasks;
using ArtemisBanking.Application.DTOs.Account;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.WebApp.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArtemisBanking.WebApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAccountService accountService,
            IMapper mapper,
            ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoleHome();
            }

            TempData.TryGetValue("AuthMessage", out var message);
            ViewBag.AuthMessage = message;
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoleHome();
            }

            if (ModelState.IsValid)
            {
                var loginDto = _mapper.Map<LoginDTO>(model);
                var authResult = await _accountService.AuthenticateAsync(loginDto);

                if (authResult.Success && authResult.User != null)
                {
                    _logger.LogInformation("Usuario autenticado: {UserName}", model.UserName);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, authResult.User.Id),
                        new Claim(ClaimTypes.Name, authResult.User.UserName ?? string.Empty),
                        new Claim(ClaimTypes.Email, authResult.User.Email ?? string.Empty)
                    };

                    foreach (var role in authResult.User.Roles ?? Array.Empty<string>())
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);

                    await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError(string.Empty, authResult.Message ?? "Intento de inicio de sesión no válido.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            _logger.LogInformation("Usuario cerro sesión");
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var requestDto = _mapper.Map<ForgotPasswordDTO>(model);
            var result = await _accountService.RequestPasswordResetAsync(requestDto);

            if (!result.Success)
            {
                TempData["AuthMessage"] = result.Message;
                return RedirectToAction(nameof(Login));
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string userName)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userName))
            {
                return View("Error");
            }

            var model = new ResetPasswordViewModel { Token = token, UserName = userName };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resetDto = _mapper.Map<ResetPasswordDTO>(model);
            var result = await _accountService.ResetPasswordAsync(resetDto);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError(string.Empty, result.Message ?? "No se pudo restablecer la contraseña");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Activate(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["AuthMessage"] = "Enlace de activación invalido";
                return RedirectToAction(nameof(Login));
            }

            var result = await _accountService.ConfirmAccountAsync(token);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["AuthMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Login));
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            
            return RedirectToRoleHome();
        }

        private IActionResult RedirectToRoleHome()
        {
            if (User.IsInRole("Administrador"))
                return RedirectToAction("Dashboard", "Admin");

            if (User.IsInRole("Cajero"))
                return RedirectToAction("Home", "Cajero");
            
            if (User.IsInRole("Cliente"))
                return RedirectToAction("Index", "Cliente");
            
            return RedirectToAction("Index", "Home");
        }
    }
}
