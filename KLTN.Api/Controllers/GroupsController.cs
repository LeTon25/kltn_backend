using AutoMapper;
using KLTN.Application.DTOs.Groups;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace KLTN.Api.Controllers
{
    public class GroupsController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        public GroupsController(ApplicationDbContext _db,
            IMapper _mapper,
            UserManager<User> userManager)
        {
            this._db = _db;
            this._mapper = _mapper;
            this._userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> GetGroupsAsync()
        {
            var query = _db.Groups;

            var groupDtos = await query.ToListAsync();
            return Ok(_mapper.Map<List<GroupDto>>(groupDtos));
        }
        [HttpGet("filter")]
        public async Task<IActionResult> GetGroupsPagingAsync(string filter, int pageIndex, int pageSize)
        {
            var query = _db.Groups.AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(e => e.GroupName.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            var data = items.Select(e => _mapper.Map<GroupDto>(e)).ToList();

            var pagination = new Pagination<GroupDto>
            {
                Items = data,
                TotalRecords = totalRecords,
                PageSize = pageSize,
                PageIndex= pageIndex,
            };
            return Ok(pagination);
        }
        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetByIdAsync(string groupId)
        {
            var group = await _db.Groups.FindAsync(groupId);
            if (group == null)
            {
                return NotFound(new ApiNotFoundResponse("Không tìm thấy nhóm cần tìm"));
            }
            return Ok(_mapper.Map<GroupDto>(group));

        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostGroupAsync(CreateGroupRequestDto requestDto)
        {
            var groups = _db.Groups;
            if (await groups.AnyAsync(c => c.GroupName.Equals(requestDto.GroupName)))
            {
                return BadRequest(new ApiBadRequestResponse("Tên nhóm không được trùng"));
            }
            var newGroupId = Guid.NewGuid();
            var newGroup = new Group()
            {
                GroupId = newGroupId.ToString(),
                GroupName = requestDto.GroupName,
                ProjectId = requestDto.ProjectId,
                NumberOfMembers = 0,
                CourseId = requestDto.CourseId, 
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
            };
            var result = await _db.AddAsync(newGroup);
            await _db.SaveChangesAsync();
            return Ok(_mapper.Map<GroupDto>(result));
        }
        [HttpPut("{groupId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutGroupId(string groupId, [FromBody] CreateGroupRequestDto requestDto)
        {
            var group = await _db.Groups.FirstOrDefaultAsync(c => c.GroupId == groupId);
            if (group == null)
            {
                return NotFound(new ApiNotFoundResponse($"Không tìm thấy nhóm với id : {groupId}"));
            }
            if (await _db.Groups.AnyAsync(e => e.GroupName == requestDto.GroupName && e.GroupId != groupId && e.CourseId == requestDto.CourseId))
            {
                return BadRequest(new ApiBadRequestResponse("Tên nhóm không được trùng"));
            }

            group.GroupName = requestDto.GroupName; 
            group.ProjectId = requestDto.ProjectId;
            group.UpdatedAt = DateTime.Now;
            group.CourseId = requestDto.CourseId;

            _db.Groups.Update(group);
            var result = await _db.SaveChangesAsync();
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse("Cập nhật nhóm thất bại"));
        }

        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DeleteGroupAsync(string groupId)
        {
            var group = await _db.Groups.FirstOrDefaultAsync(c => c.GroupId == groupId);
            if (group == null)
            {
                return NotFound(new ApiNotFoundResponse("Không thể tìm thấy nhóm với id"));
            }
            _db.Groups.Remove(group);
            var result = await _db.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(_mapper.Map<GroupDto>(group));
            }
            return BadRequest(new ApiBadRequestResponse("Xóa thông tin nhóm thất bại"));
        }

        [HttpPost("{groupId}/add-members")]
        public async Task<IActionResult> PostAddMembersToGroup(string groupId, AddMemberToGroupDto requestDto)
        {
            var group = await _db.Groups.FindAsync(groupId);
            if (group == null) 
            { 
                return NotFound(new ApiNotFoundResponse("Không tìm thấy nhóm"));
            } 
            if (requestDto.studentIds.Length > 0 && !string.IsNullOrEmpty(requestDto.leaderId)) { 
                foreach(var id in requestDto.studentIds)
                {
                    var student = await _userManager.FindByIdAsync(id);
                    if (student != null) 
                    {
                        await _db.GroupMembers.AddAsync(new GroupMember()
                        {
                            GroupId =groupId ,
                            StudentId = student.Id,
                            IsLeader = student.Id == requestDto.leaderId,
                        });
                    }
                }
                group.NumberOfMembers += requestDto.studentIds.Length;
            }
            await _db.SaveChangesAsync();
            return NoContent();
        }

    }
}
