﻿using KLTN.Application.DTOs.Assignments;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AssignmentsController : ControllerBase
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
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await assignmentService.GetAssignmentByIdAsync(assignmentId, userId!);
            return StatusCode(response.StatusCode,response);

        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostAssignmentAsync(UpSertAssignmentRequestDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await assignmentService.CreateAssignmentAsync(userId!, requestDto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPatch("{assignmentId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutAssignmentId(string assignmentId, [FromBody] UpSertAssignmentRequestDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await assignmentService.UpdateAssignmentAsync(userId!, assignmentId, requestDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{assignmentId}")]
        public async Task<IActionResult> DeleteAssignmentAsync(string assignmentId)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await assignmentService.DeleteAssignmentAsync(userId!, assignmentId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{assignmentId}/submissions")]
        public async Task<IActionResult> GetSubmissionsInAssignment(string assignmentId)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await assignmentService.GetSubmissionsInAssignmentsAsync(userId!,assignmentId);
            return StatusCode(response.StatusCode,response);
        }
        [HttpGet("current-user")]
        public async Task<IActionResult> GetAsignmentsByCurrentUser()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await assignmentService.GetAsignmentsByCurrentUserAsync(userId!);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{assignmentId}/downloads")]
        public async Task<IActionResult> GetFileSubmissionAsync(string assignmentId)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await assignmentService.GetFileSubmissionAsync(assignmentId,userId!);
            return StatusCode(response.StatusCode,response);
        }
        [HttpGet("{assignmentId}/studentsWithoutGroup")]
        public async Task<IActionResult> GetStudentWithoutGroupAsync(string assignmentId)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await assignmentService.GetStudentWithoutGroupAsync(assignmentId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
