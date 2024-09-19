using AutoMapper;
using KLTN.Api.Services.Interfaces;
using KLTN.Application.DTOs.Assignments;
using KLTN.Application.DTOs.Announcements;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Semesters;
using KLTN.Application.DTOs.Subjects;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Application.Services;
using KLTN.Domain;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class AssignmentsController : BaseController
    {
        private readonly AssignmentService assignmentService;
        public AssignmentsController(ApplicationDbContext db,
            IMapper mapper,
            IStorageService storageService,
            AssignmentService assignmentService
            )
        {
            this.assignmentService = assignmentService;
        }
        [HttpGet("{assignmentId}")]
        public async Task<IActionResult> GetByIdAsync(string assignmentId)
        {
            return SetResponse(await assignmentService.GetAssignmentByIdAsync(assignmentId));

        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostAnnouncementAsync(UpSertAssignmentRequestDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await assignmentService.CreateAssignmentAsync(userId,requestDto));
        }
        [HttpPatch("{assignmentId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutAnnouncementId(string assignmentId, [FromBody] UpSertAssignmentRequestDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await assignmentService.UpdateAssignmentAsync(userId,assignmentId, requestDto));
        }

        [HttpDelete("{assignmentId}")]
        public async Task<IActionResult> DeleteAnnouncementAsync(string assignmentId)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await assignmentService.DeleteAssignmentAsync(userId,assignmentId));
        }
    }
}
