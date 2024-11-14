using KLTN.Api.Filters;
using KLTN.Application.DTOs.ScoreStructures;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class ScoreStructuresController : BaseController
    {
        private readonly ScoreStructureService _scoreStructureService;
        public ScoreStructuresController(ScoreStructureService scoreStructureService)
        {
            _scoreStructureService = scoreStructureService;
        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> SaveScoreStructureAsync(UpSertScoreStructureDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _scoreStructureService.SaveScoreStrucutureAsync(userId!,requestDto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetScoreStructureByIdAsync(string id)
        {
            var response = await _scoreStructureService.GetScoreStructureByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("course/{courseId}/score-structure")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        public async Task<IActionResult> GetScoreStructureByCourseIdAsync(string courseId)
        {
            var response = await _scoreStructureService.GetScoreStructureByCourseIdAsync(courseId);
            return StatusCode(response.StatusCode, response);   
        }
        [HttpGet("{courseId}/transcripts")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        public async Task<IActionResult> GetTranscriptAsync(string courseId)
        {
            var response = await _scoreStructureService.GetTransciptAsync(courseId);
            return StatusCode(response.StatusCode,response);
        }
        [HttpGet("{courseId}/transcripts/statistics")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        public async Task<IActionResult> GetTranscriptStatisticsAsync(string courseId)
        {
            var response = await _scoreStructureService.GetTransciptStatisticsAsync(courseId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
