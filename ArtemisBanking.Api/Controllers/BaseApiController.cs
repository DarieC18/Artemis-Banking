using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Api.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
    }
}
