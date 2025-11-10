using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.WebApp.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClienteController : Controller
    {
        [HttpGet]
        public IActionResult Home()
        {
            return View();
        }
    }
}
