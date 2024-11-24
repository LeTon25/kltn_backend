using KLTN.Application.DTOs.Subjects;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class SubjectsController : BaseController
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
            return SetResponse(apiResposne);

        }
        [HttpPost]
        [ApiValidationFilter]
        [RoleRequirement(["Admin"])]

        public async Task<IActionResult> PostSubjectAsync(CreateSubjectRequestDto requestDto)
        {
            return SetResponse(await _subjectService.AddSubjectAsync(requestDto));
        }
        [HttpPut("{subjectId}")]
        [ApiValidationFilter]
        [RoleRequirement(["Admin"])]
        public async Task<IActionResult> PutSubjectId(string subjectId, [FromBody] CreateSubjectRequestDto requestDto)
        {
            return SetResponse(await _subjectService.UpdateSubjectAsync(subjectId, requestDto));
        }

        [HttpDelete("{subjectId}")]
        [RoleRequirement(["Admin"])]

        public async Task<IActionResult> DeleteSubjectAsync(string subjectId)
        {
            return SetResponse(await _subjectService.DeleteSubjectAsync(subjectId));
        }

    }
}
