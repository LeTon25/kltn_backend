using AutoMapper;
using KLTN.Api.Services.Interfaces;
using KLTN.Application.DTOs.Announcements;
using KLTN.Application.DTOs.Uploads;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Application.Services;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.Diagnostics.Eventing.Reader;
using System.Net.Mail;
using System.Net.WebSockets;
using File = KLTN.Domain.Entities.File;

namespace KLTN.Api.Controllers
{
    public class AnnouncementsController : BaseController
    {
        private readonly AnnoucementService annoucementService;
        public AnnouncementsController(ApplicationDbContext db,
            IMapper mapper,
            IStorageService storageService,
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
