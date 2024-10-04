using AutoMapper;
using KLTN.Api.Services.Interfaces;
using KLTN.Application.DTOs.Submissions;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class SubmissionsController : BaseController
    {
        private readonly SubmissionService submissionService;
        public SubmissionsController(
            SubmissionService submissionService
            )
        {
            this.submissionService = submissionService;
        }
        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetByIdAsync(string submissionId)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await submissionService.GetSubmissionByIdAsync(submissionId,userId);
            return StatusCode(response.StatusCode,response);
        }
        [HttpPost("{assignmentId}/submissions")]
        [ApiValidationFilter]
        public async Task<IActionResult> PostSubmissionAsync(CreateSubmissionDto requestDto,string assignmentId)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await submissionService.CreateSubmissionAsync(userId,requestDto,assignmentId));
        }
        [HttpPatch("{assignmentId}/submissions/{submissionId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutSubmission(string submissionId, [FromBody] CreateSubmissionDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await submissionService.UpdateSubmissionAsync(userId,submissionId, requestDto));
        }

        [HttpDelete("{assignmentId}/submissions/{submissionId}")]
        public async Task<IActionResult> DeleteSubmissionAsync(string submissionId)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await submissionService.DeleteSubmissionAsync(userId,submissionId));
        }
    }
}
