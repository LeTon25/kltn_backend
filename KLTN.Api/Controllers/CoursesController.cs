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
    [Authorize]
    public class CoursesController : BaseController
    {
        private readonly CourseService _courseService;
        public CoursesController(
            CourseService courseService,
            AssignmentService assignmentService
          ) 
        { 
            this._courseService = courseService;
        }
        [HttpGet("{courseId}")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        public async Task<IActionResult> GetByIdAsync(string courseId)
        {
            return SetResponse( await _courseService.GetCourseByIdAsync(courseId));
        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostCourseAsync(CreateCourseRequestDto requestDto)
        {
            return SetResponse(await _courseService.CreateCourseAsync(requestDto));
        }
        [HttpPatch("{courseId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutCourseId(string courseId, [FromBody] CreateCourseRequestDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await _courseService.UpdateCourseAsync(courseId,requestDto,userId));
        }

        [HttpPatch("{courseId}/inviteCode")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        [ApiValidationFilter]
        public async Task<IActionResult> PutCourseInviteCodeAsync(string courseId,[FromBody]string inviteCode)
        {
            return SetResponse(await _courseService.UpdateInviteCodeAsync(courseId, inviteCode));
        }
        [HttpGet("invite/{inviteCode:length(1,100)}")]
        public async Task<IActionResult> GetFindCourseByInviteCodeAsync(string inviteCode)
        {
            return SetResponse(await _courseService.GetFindCourseByInviteCodeAsync(inviteCode));
        }
        [HttpPost("invite/{inviteCode:length(1,100)}")]
        public async Task<IActionResult> GetApplyCodeAsync( string inviteCode)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await _courseService.ApplyInviteCodeAsync(inviteCode, userId));
        }
        [HttpGet("{courseId}/regenerateCode")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        public async Task<IActionResult> GetRegenerateInviteCodeAsync(string courseId)
        {
            return SetResponse(await _courseService.GetRegenerateInviteCodeAsync(courseId));
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
            return SetResponse(await _courseService.GetSuggestInviteCodeAsync());
        }
        [HttpGet("{courseId}/hide-code")]
        public async Task<IActionResult> GetHideSuggestCodeAsync(string courseId)
        {
            return SetResponse(await _courseService.GetToggleInviteCodeAsync(courseId,true));
        }
        [HttpGet("{courseId}/show-code")]
        public async Task<IActionResult> GetShowSuggestCodeAsync(string courseId)
        {
            return SetResponse(await _courseService.GetToggleInviteCodeAsync(courseId,false));
        }
        [HttpDelete("{courseId}")]
        public async Task<IActionResult> DeleteCourseAsync(string courseId)
        {
            return SetResponse( await _courseService.DeleteCourseAsync(courseId));
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
            return SetResponse(data);
        }
        [HttpGet("{courseId}/projects")]
        [ServiceFilter(typeof(CourseResourceAccessFilter))]
        public async Task<IActionResult> GetProjectsInCourseAsync(string courseId)
        {
            return SetResponse(await _courseService.GetProjectsInCourseAsync(courseId));
        }
        [HttpDelete("{courseId}/students")]
        public async Task<IActionResult> DeleteRemoveStudentFromCourseAsync(string courseId,RemoveStudentRequestDto dto)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await _courseService.RemoveStudentFromCourseAsync(courseId,dto.StudentIds,currentUserId));
        }
        [HttpPost("{courseId}/students")]
        public async Task<IActionResult> AddStudentsToCourseAsync(string courseId, AddStudentRequestDto dto)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await _courseService.AddStudentToCourseAsync(courseId, dto, currentUserId));
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
        //[HttpPost("{courseId}/import-student")]
        //public async Task<IActionResult> ImportStudentsAsync()
        //{
        //    string currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        //}

    }
}
