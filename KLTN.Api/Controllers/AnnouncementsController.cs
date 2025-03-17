using KLTN.Application.DTOs.Announcements;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KLTN.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnnouncementsController : ControllerBase
    {
        private readonly AnnoucementService annoucementService;
        public AnnouncementsController(
            AnnoucementService annoucementService
            ) {
            this.annoucementService = annoucementService;
        }
        [HttpPatch("{announcementId}/pinned")]
        public async Task<IActionResult> PatchAnnouncementsAsync(string announcementId,bool isPinned)
        {
            var response = await annoucementService.TogglePinAnnouncement(announcementId, isPinned);
            return StatusCode(response.StatusCode,response);
            
        }
        [HttpGet("{announcementId}")]
        public async Task<IActionResult> GetByIdAsync(string announcementId)
        {
            var response = await annoucementService.GetAnnouncementByIdAsync(announcementId);
            return StatusCode(response.StatusCode, response);

        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostAnnouncementAsync(CreateAnnouncementRequestDto requestDto)
        {
            var response = await annoucementService.CreateAnnouncementAsync(requestDto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPatch("{courseId}/{announcementId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutAnnouncementId(string announcementId, [FromBody] CreateAnnouncementRequestDto requestDto)
        {
            var response = await annoucementService.UpdateAnnouncementAsync(announcementId, requestDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{courseId}/{announcementId}")]
        public async Task<IActionResult> DeleteAnnouncementAsync(string announcementId)
        {
            var response = await annoucementService.DeleteAnnouncementAsync(announcementId);
            return StatusCode(response.StatusCode, response);
        }

    }
}
