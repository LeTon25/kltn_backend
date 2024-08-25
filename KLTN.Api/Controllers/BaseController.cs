using KLTN.Application.Helpers.Response;
using Microsoft.AspNetCore.Mvc;

namespace KLTN.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController : ControllerBase
    {
        protected IActionResult SetResponse(ApiResponse<object> api)
        {
            switch (api.StatusCode)
            {
                case 200:
                    return Ok(api);
                case 400:
                    return BadRequest(api);
                case 401:
                    return Unauthorized(api);
                case 404:
                    return NotFound(api);
                default:
                    return Ok(api);
            }
        }
    }
}
