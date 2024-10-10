using KLTN.Application.DTOs.Scores;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ScoresController : ControllerBase
    {
        private readonly ScoreServices scoreServices;
        public ScoresController(ScoreServices scoreServices)
        {
            this.scoreServices = scoreServices; 
        }
        [HttpPost("{submissionId}/score")]
        [ApiValidationFilter]
        public async Task<IActionResult> ScoringSubmissionAsync(CreateScoreDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await scoreServices.ScoringSubmissionAsync(requestDto,userId);
            return StatusCode(response.StatusCode,response);
        }
        [HttpPatch("{scoreId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> UpdateScoreAsync(UpdateScoreDto requestDto,string scoreId)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await scoreServices.UpdateScoreAsync(requestDto,scoreId ,userId);
            return StatusCode(response.StatusCode,response);    
        }
    }
}
