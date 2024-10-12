using KLTN.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestsController : ControllerBase
    {
        //[HttpPost("{groupId}/make-request")]
        //public async Task<IActionResult> MakeRequestToJoinAsync(string groupId)
        //{
        //    var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var response = await groupService.MakeRequestToJoinAsync(groupId, currentUserId);
        //    return StatusCode(response.StatusCode, response);
        //}
    }
}
