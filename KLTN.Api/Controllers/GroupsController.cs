using AutoMapper;
using KLTN.Application.DTOs.Groups;
using KLTN.Application.DTOs.Projects;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Application.Services;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class GroupsController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly GroupService groupService; 
        public GroupsController(ApplicationDbContext _db,
            IMapper _mapper,
            UserManager<User> userManager,
            GroupService groupService)
        {
            this._db = _db;
            this._mapper = _mapper;
            this._userManager = userManager;
            this.groupService = groupService;
        }
        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetByIdAsync(string groupId)
        {
            return SetResponse(await groupService.GetByIdAsync(groupId));
        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostGroupAsync(CreateGroupRequestDto requestDto)
        {
            return SetResponse(await groupService.PostGroupAsync(requestDto));
        }
        [HttpPatch("{groupId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutGroupIdAsync(string groupId, [FromBody] CreateGroupRequestDto requestDto)
        {
            return SetResponse(await groupService.PutGroupAsync(groupId,requestDto));
        }

        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DeleteGroupAsync(string groupId)
        {
            return SetResponse(await groupService.DeleteGroupAsync(groupId));
        }

        [HttpPost("{groupId}/members")]
        public async Task<IActionResult> PostAddMembersToGroupAsync(string groupId, AddMemberToGroupDto requestDto)
        {
            return SetResponse(await groupService.PostAddMembersToGroupAsync(groupId, requestDto));

        }
        [HttpDelete("{groupId}/members")]
        public async Task<IActionResult> DeleteRemoveMemberAsync(string groupId,RemoveMemberFromGroupDto requestDto)
        {
            return SetResponse(await groupService.DeleteRemoveMemberAsync(groupId, requestDto));
        }
    }
}
