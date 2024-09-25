using AutoMapper;
using KLTN.Api.Services.Interfaces;
using KLTN.Application.DTOs.Assignments;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class AssignmentsController : BaseController
    {
        private readonly AssignmentService assignmentService;
        public AssignmentsController(
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
