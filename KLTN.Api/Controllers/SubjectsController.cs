using KLTN.Application.DTOs.Subjects;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KLTN.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubjectsController : ControllerBase
    {
        private readonly SubjectService _subjectService;    
        public SubjectsController(SubjectService subjectService) {
            this._subjectService = subjectService;
        }
        [HttpGet]
        public async Task<IActionResult> GetSubjectsAsync()
        {
            return  Ok(await _subjectService.GetAllSubjectAsync());
        }
        [HttpGet("{subjectId}")]
        public async Task<IActionResult> GetByIdAsync(string subjectId)
        {
            var apiResposne = await _subjectService.GetByIdAsync(subjectId);
            return StatusCode(apiResposne.StatusCode, apiResposne);
        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostSubjectAsync(CreateSubjectRequestDto requestDto)
        {
            var apiResposne = await _subjectService.AddSubjectAsync(requestDto);
            return StatusCode(apiResposne.StatusCode, apiResposne);
        }
        [HttpPut("{subjectId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutSubjectId(string subjectId, [FromBody] CreateSubjectRequestDto requestDto)
        {
            var apiResposne = await _subjectService.AddSubjectAsync(requestDto);
            return StatusCode(apiResposne.StatusCode, apiResposne);
        }

        [HttpDelete("{subjectId}")]
        public async Task<IActionResult> DeleteSubjectAsync(string subjectId)
        {
            var apiResposne = await _subjectService.DeleteSubjectAsync(subjectId);
            return StatusCode(apiResposne.StatusCode, apiResposne);
        }

    }
}
