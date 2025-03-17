using KLTN.Api.Filters;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Groups;
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
    public class CoursesController : ControllerBase
    {
        private readonly CourseService _courseService;
        public CoursesController(
            CourseService courseService,
            AssignmentService assignmentService
          ) 
        { 
            this._courseService = courseService;
        }
        [HttpGet]
        [RoleRequirement(["Admin"])]
        public async Task<IActionResult> GetAllCoursesAsync()
        {
            var response = await _courseService.GetAllCoursesAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{courseId}")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        public async Task<IActionResult> GetByIdAsync(string courseId)
        {
            var response = await _courseService.GetCourseByIdAsync(courseId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostCourseAsync(CreateCourseRequestDto requestDto)
        {
            var response = await _courseService.CreateCourseAsync(requestDto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("template")]
        [ApiValidationFilter]
        public async Task<IActionResult> PostCourseFromTemplateAsync(CreateCourseFromTemplateDto requestDto)
        {
            var response = await _courseService.CreateCourseFromTemplateAsync(requestDto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPatch("{courseId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutCourseId(string courseId, [FromBody] CreateCourseRequestDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _courseService.UpdateCourseAsync(courseId, requestDto, userId!);
            return StatusCode(response.StatusCode,response);
        }

        [HttpPatch("{courseId}/inviteCode")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        [ApiValidationFilter]
        public async Task<IActionResult> PutCourseInviteCodeAsync(string courseId,[FromBody]string inviteCode)
        {
            var response = await _courseService.UpdateInviteCodeAsync(courseId, inviteCode);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("invite/{inviteCode:length(1,100)}")]
        public async Task<IActionResult> GetFindCourseByInviteCodeAsync(string inviteCode)
        {
            var response = await _courseService.GetFindCourseByInviteCodeAsync(inviteCode);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("invite/{inviteCode:length(1,100)}")]
        public async Task<IActionResult> GetApplyCodeAsync( string inviteCode)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _courseService.ApplyInviteCodeAsync(inviteCode, userId!);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{courseId}/regenerateCode")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        public async Task<IActionResult> GetRegenerateInviteCodeAsync(string courseId)
        {
            var response = await _courseService.GetRegenerateInviteCodeAsync(courseId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{courseId}/studentsWithoutGroup")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        public async Task<IActionResult> GetStudentsWithoutGroupsAsync(string courseId)
        {
            var response = await _courseService.GetStudentsWithoutGroupsAsync(courseId);
            return StatusCode(response.StatusCode,response);
        }
        [HttpGet("suggest-inviteCode")]
        public async Task<IActionResult> GetRenerateInviteCodeAsync()
        {
            var response = await _courseService.GetSuggestInviteCodeAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{courseId}/hide-code")]
        public async Task<IActionResult> GetHideSuggestCodeAsync(string courseId)
        {
            var response = await _courseService.GetToggleInviteCodeAsync(courseId, true);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{courseId}/show-code")]
        public async Task<IActionResult> GetShowSuggestCodeAsync(string courseId)
        {
            var response = await _courseService.GetToggleInviteCodeAsync(courseId, true);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete("{courseId}")]
        public async Task<IActionResult> DeleteCourseAsync(string courseId)
        {
            var response = await _courseService.DeleteCourseAsync(courseId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{courseId}/groups")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        public async Task<IActionResult> GetCourseGroupsAsync(string courseId)
        {
            var data = await _courseService.GetGroupsInCourseAsync(courseId);
            if(data.StatusCode == 200)
            {
                var course = await _courseService.GetCourseDtoByIdAsync(courseId,false,false,false,false);
                foreach (var group in data.Data as List<GroupDto>)
                {
                    group.Course = course;
                }
            }
            return StatusCode(data.StatusCode, data);

        }
        [HttpGet("{courseId}/projects")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        public async Task<IActionResult> GetProjectsInCourseAsync(string courseId)
        {
            var response = await _courseService.GetProjectsInCourseAsync(courseId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete("{courseId}/students")]
        public async Task<IActionResult> DeleteRemoveStudentFromCourseAsync(string courseId,RemoveStudentRequestDto dto)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _courseService.RemoveStudentFromCourseAsync(courseId, dto.StudentIds, currentUserId!);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("{courseId}/students")]
        public async Task<IActionResult> AddStudentsToCourseAsync(string courseId, AddStudentRequestDto dto)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _courseService.AddStudentToCourseAsync(courseId, dto, currentUserId!);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{courseId}/statistic")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        public async Task<IActionResult> GetStatisticAsync(string courseId)
        {
            var response = await _courseService.GetStatisticAsync(courseId);
            return StatusCode(response.StatusCode,response);
        }
        [HttpGet("{courseId}/end-term")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        public async Task<IActionResult> GetEndtermAsync(string courseId)
        {
            string currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var response = await _courseService.GetEndTermAsync(courseId,currentUserId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("{courseId}/saved")]
        public async Task<IActionResult> PostArchiveAsync(string courseId)
        {
            string currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var response = await _courseService.ArchiveCourseAsync(courseId, currentUserId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("{courseId}/cancel-saved")]
        public async Task<IActionResult> PostCancelArchiveAsync(string courseId)
        {
            string currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var response = await _courseService.CancelArchiveCourseAsync(courseId, currentUserId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("{courseId}/import-student")]
        public async Task<IActionResult> ImportStudentsAsync(string courseId,[FromBody]List<ImportStudent> dto)
        {
            string currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var response = await _courseService.ImportStudentsToCourseAsync(courseId, dto, currentUserId);
            return StatusCode(response.StatusCode, response);
        }

    }
}
