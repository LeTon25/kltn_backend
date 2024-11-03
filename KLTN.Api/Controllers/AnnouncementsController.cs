using KLTN.Application.DTOs.Announcements;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class AnnouncementsController : BaseController
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
            return SetResponse(await annoucementService.TogglePinAnnouncement(announcementId, isPinned));
            
        }
        [HttpGet("{announcementId}")]
        public async Task<IActionResult> GetByIdAsync(string announcementId)
        {
            return SetResponse(await annoucementService.GetAnnouncementByIdAsync(announcementId));  

        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostAnnouncementAsync(CreateAnnouncementRequestDto requestDto)
        {
            return SetResponse(await annoucementService.CreateAnnouncementAsync(requestDto));
        }
        [HttpPatch("{announcementId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutAnnouncementId(string announcementId, [FromBody] CreateAnnouncementRequestDto requestDto)
        {
            return SetResponse(await annoucementService.UpdateAnnouncementAsync(announcementId, requestDto));
        }

        [HttpDelete("{announcementId}")]
        public async Task<IActionResult> DeleteAnnouncementAsync(string announcementId)
        {
            return SetResponse(await annoucementService.DeleteAnnouncementAsync(announcementId));
        }

    }
}
