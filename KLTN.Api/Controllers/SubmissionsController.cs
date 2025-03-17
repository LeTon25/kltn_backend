using KLTN.Application.DTOs.Submissions;
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
    public class SubmissionsController : ControllerBase
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
            var response = await submissionService.CreateSubmissionAsync(userId!, requestDto, assignmentId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPatch("{assignmentId}/submissions/{submissionId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutSubmission(string submissionId, [FromBody] CreateSubmissionDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await submissionService.UpdateSubmissionAsync(userId!, submissionId, requestDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{assignmentId}/submissions/{submissionId}")]
        public async Task<IActionResult> DeleteSubmissionAsync(string submissionId)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await submissionService.DeleteSubmissionAsync(userId!, submissionId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
