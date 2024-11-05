using KLTN.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestsController : ControllerBase
    {
        private readonly RequestService requestService;
        public RequestsController(RequestService requestService)
        { 
            this.requestService = requestService;
        }  
        [HttpPost("{groupId}/make-request")]
        public async Task<IActionResult> MakeRequestToJoinAsync(string groupId)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await requestService.MakeRequestToJoinAsync(groupId, currentUserId!);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete("{groupId}/remove-request")]
        public async Task<IActionResult> RemoveRequestToJoinAsync(string groupId)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await requestService.DeleteRequestToJoinAsync(groupId, currentUserId!);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("{groupId}/accept-request")]
        public async Task<IActionResult> AcceptRequestToJoinAsync(string groupId)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await requestService.AcceptRequestToJoinAsync(groupId, currentUserId!);
            return StatusCode(response.StatusCode, response);
        }
    }
}
