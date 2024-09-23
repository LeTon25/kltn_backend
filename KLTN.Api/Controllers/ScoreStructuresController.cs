using KLTN.Application.DTOs.Assignments;
using KLTN.Application.DTOs.ScoreStructures;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using KLTN.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    //[ApiController]
    //[Route("api/[controller]")]
    //public class ScoreStructuresController : BaseController
    //{
    //    private readonly ScoreStructureService _scoreStructureService;
    //    public ScoreStructuresController(ScoreStructureService scoreStructureService)
    //    {
    //        _scoreStructureService = scoreStructureService;
    //    }

    //    [HttpGet("{scoreStructureId}")]
    //    public async Task<IActionResult> GetByIdAsync(string scoreStructureId)
    //    {
    //        return SetResponse(await _scoreStructureService.GetScoreStructureAsync(scoreStructureId));

    //    }
    //    [HttpPost]
    //    [ApiValidationFilter]
    //    public async Task<IActionResult> PostScoreStructureAsync(CreateScoreStuctureRequestDto requestDto)
    //    {
    //        var response = await _scoreStructureService.CreateScoreStructureAsync(requestDto);
    //        return SetResponse(response);
    //    }
    //    [HttpPatch("{scoreStructureId}")]
    //    [ApiValidationFilter]
    //    public async Task<IActionResult> PutScoreStructureId(string scoreStructureId, [FromBody] CreateScoreStuctureRequestDto requestDto)
    //    {
    //        var response = await _scoreStructureService.UpdateScoreStructureAsync(scoreStructureId,requestDto);
    //        return SetResponse(response);

    //    }

    //    [HttpDelete("{scoreStructureId}")]
    //    public async Task<IActionResult> DeleteScoreStructureAsync(string scoreStructureId)
    //    {
    //        var response = await _scoreStructureService.DeleteScoreStructureAsync(scoreStructureId);    
    //        return SetResponse(response);
    //    }
    //}
}
