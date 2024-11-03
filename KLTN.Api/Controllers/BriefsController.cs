using KLTN.Application.DTOs.Briefs;
using KLTN.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BriefsController : ControllerBase
    {
        private readonly BriefService briefService;
        public BriefsController(BriefService briefService)
        {
            this.briefService = briefService;
        }

        [HttpGet("{groupId}/brief")]
        public async Task<IActionResult> GetBriefsInGroupAsync(string groupId)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await briefService.GetBriefsInGroupAsync(groupId,currentUserId);

            return StatusCode(response.StatusCode,response);
        }
        [HttpPost("{groupId}/brief")]
        public async Task<IActionResult> PostBriefsAsync(CreateBriefDto dto,string groupId)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await briefService.CreateBriefAsync(dto,groupId, currentUserId);

            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete("{groupId}/brief/{briefId}")]
        public async Task<IActionResult> DeleteBriefsAsync(string groupId,string briefId)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await briefService.DeleteBriefAsync(briefId,groupId,currentUserId);

            return StatusCode(response.StatusCode, response);
        }
        [HttpPatch("{groupId}/brief/{briefId}")]
        public async Task<IActionResult> DeleteBriefsAsync(CreateBriefDto dto,string groupId, string briefId)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await briefService.UpdateBriefAsync(dto,briefId, groupId, currentUserId);

            return StatusCode(response.StatusCode, response);
        }
    }
}
