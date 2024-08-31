using Amazon.S3.Model.Internal.MarshallTransformations;
using AutoMapper;
using KLTN.Application.DTOs.Announcements;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Groups;
using KLTN.Application.DTOs.Projects;
using KLTN.Application.DTOs.Semesters;
using KLTN.Application.DTOs.Subjects;
using KLTN.Application.DTOs.Uploads;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Application.Services;
using KLTN.Domain;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class CoursesController : BaseController
    {
        private readonly CourseService _courseService;
        public CoursesController(ApplicationDbContext _db,
            IMapper _mapper,
            CourseService courseService
          ) 
        { 
            this._courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCoursesAsync()
        {
            return SetResponse(await _courseService.GetAllCoursesAsync());
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            return SetResponse( await _courseService.GetCourseByIdAsync(id));
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
            return SetResponse(await _courseService.UpdateCourseAsync(courseId,requestDto));
        }

        [HttpPatch("{courseId}/inviteCode")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutCourseInviteCodeAsync(string courseId,[FromBody]string inviteCode)
        {
            return SetResponse(await _courseService.UpdateInviteCodeAsync(courseId, inviteCode));
        }

        [HttpGet("invite/{inviteCode:length(1,100)}")]
        public async Task<IActionResult> GetApplyCodeAsync(string courseId, string inviteCode)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await _courseService.ApplyInviteCodeAsync(courseId, inviteCode, userId));
        }

        [HttpDelete("{courseId}")]
        public async Task<IActionResult> DeleteCourseAsync(string courseId)
        {
            return SetResponse( await _courseService.DeleteCourseAsync(courseId));
        }

        [HttpGet("{courseId}/groups")]
        public async Task<IActionResult> GetCourseGroupsAsync(string courseId)
        {
            return SetResponse(await _courseService.GetGroupsInCourseAsync(courseId));
        }
        [HttpGet("{courseId}/projects")]
        public async Task<IActionResult> GetProjectsInCourseAsync(string courseId)
        {
            return SetResponse(await _courseService.GetProjectsInCourseAsync(courseId));
        }
    }
}
