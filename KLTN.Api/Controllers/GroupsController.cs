﻿using KLTN.Api.Filters;
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
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GroupsController : ControllerBase
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
            return StatusCode(data.StatusCode,data);
        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostGroupAsync(CreateGroupRequestDto requestDto)
        {
            var data = await groupService.PostGroupAsync(requestDto);
            if (data.StatusCode == 200)
            {
                var courseDto = await courseService.GetCourseDtoByIdAsync((data.Data as GroupDto).CourseId,false,false,false,false);
                (data.Data as GroupDto).Course = courseDto;
            }
            return StatusCode(data.StatusCode,data);
        }
        [HttpPatch("{groupId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutGroupIdAsync(string groupId, [FromBody] CreateGroupRequestDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var data = await groupService.PutGroupAsync(groupId, requestDto, userId!);
            if (data.StatusCode == 200)
            {
                var courseDto = await courseService.GetCourseDtoByIdAsync((data.Data as GroupDto).CourseId,false,false,false,false);
                (data.Data as GroupDto).Course = courseDto;
            }
            return StatusCode(data.StatusCode,data);
        }

        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DeleteGroupAsync(string groupId)
        {
            var response = await groupService.DeleteGroupAsync(groupId);
            return StatusCode(response.StatusCode,response);
        }

        [HttpPost("{groupId}/members")]
        public async Task<IActionResult> PostAddMembersToGroupAsync(string groupId, AddMemberToGroupDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await groupService.PostAddMembersToGroupAsync(groupId, requestDto, userId!);
            return StatusCode(response.StatusCode,response);

        }
        [HttpDelete("{groupId}/members")]
        public async Task<IActionResult> DeleteRemoveMemberAsync(string groupId,RemoveMemberFromGroupDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await groupService.DeleteRemoveMemberAsync(groupId, requestDto, userId!);
            return StatusCode(response.StatusCode, response);
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
        [HttpPost("{courseId}/auto-generate")]
        public async Task<IActionResult> PostAutoGenerateGroupAsync(string courseId,AutoGenerateGroupDto requestDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await groupService.AutoGenerateGroupAsync(courseId,requestDto,userId!);
            return StatusCode(response.StatusCode,response);
        }

    }
}
