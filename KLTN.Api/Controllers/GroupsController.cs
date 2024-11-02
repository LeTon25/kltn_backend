using Amazon.S3.Model.Internal.MarshallTransformations;
using AutoMapper;
using KLTN.Api.Filters;
using KLTN.Application.DTOs.Groups;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class GroupsController : BaseController
    {
        private readonly GroupService groupService;
        private readonly CourseService courseService;
        public GroupsController(ApplicationDbContext _db,
            UserManager<User> userManager,
            GroupService groupService,
            CourseService courseService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.groupService = groupService;
            this.courseService = courseService;
        }
        [HttpGet("{groupId}")]
        [ServiceFilter(typeof(GroupResourceAccessFilter))]
        public async Task<IActionResult> GetByIdAsync(string groupId)
        {
            var data = await groupService.GetByIdAsync(groupId);
            if(data.StatusCode == 200)
            {
                var courseDto = await courseService.GetCourseDtoByIdAsync((data.Data as GroupDto).CourseId,false,false,false,false);
                (data.Data as GroupDto).Course = courseDto;
            }
            return SetResponse(data);
        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostGroupAsync(CreateGroupRequestDto requestDto)
        {
            var data = await groupService.PostGroupAsync(requestDto);
            if (data.StatusCode == 200)
            {
                var courseDto = await courseService.GetCourseDtoByIdAsync((data.Data as GroupDto).CourseId);
                (data.Data as GroupDto).Course = courseDto;
            }
            return SetResponse(data);
        }
        [HttpPatch("{groupId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutGroupIdAsync(string groupId, [FromBody] CreateGroupRequestDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var data = await groupService.PutGroupAsync(groupId, requestDto, userId!);
            if (data.StatusCode == 200)
            {
                var courseDto = await courseService.GetCourseDtoByIdAsync((data.Data as GroupDto).CourseId);
                (data.Data as GroupDto).Course = courseDto;
            }
            return SetResponse(data);
        }

        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DeleteGroupAsync(string groupId)
        {
            return SetResponse(await groupService.DeleteGroupAsync(groupId));
        }

        [HttpPost("{groupId}/members")]
        public async Task<IActionResult> PostAddMembersToGroupAsync(string groupId, AddMemberToGroupDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await groupService.PostAddMembersToGroupAsync(groupId, requestDto,userId));

        }
        [HttpDelete("{groupId}/members")]
        public async Task<IActionResult> DeleteRemoveMemberAsync(string groupId,RemoveMemberFromGroupDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await groupService.DeleteRemoveMemberAsync(groupId, requestDto,userId));
        }

        [HttpPost("{groupId}/leader")]
        public async Task<IActionResult> AssignLeaderAsync(string groupId ,[FromBody]ChangeLeaderRequestDto dto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await groupService.AssignLeaderAsync(userId!,groupId,dto.StudentId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{groupId}/report")]
        [ServiceFilter(typeof(GroupResourceAccessFilter))]
        public async Task<IActionResult> GetReportInGroupAsync(string groupId)
        {
            var response = await groupService.GetReportsInGroupAsync(groupId);
            return StatusCode(response.StatusCode,response);
        }
   
    }
}
