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
            var query = from g in _db.Groups
                        join project in _db.Projects on g.ProjectId equals project.ProjectId into gProject
                        from project in gProject.DefaultIfEmpty()
                        
                        join course in _db.Courses on g.CourseId equals course.CourseId into gCourse
                        from course in gCourse.DefaultIfEmpty()
                        select new GroupDto
                        {
                            GroupId= g.GroupId,
                            GroupName=g.GroupName,
                            ProjectId=project != null ? project.Title : "Chưa đăng ký",
                            CourseId=g.CourseId,
                            NumberOfMembers=g.NumberOfMembers,
                            CreatedAt=g.CreatedAt,
                            UpdatedAt=g.UpdatedAt,
                            DeletedAt=g.DeletedAt,
                            CourseGroup= course != null ? course.CourseGroup :"Không tìm thấy"
                        };

            var groupDtos = await query.ToListAsync();
            return Ok(new ApiResponse<List<GroupDto>>(200,"Thành công", groupDtos));
        }
        [HttpGet("filter")]
        public async Task<IActionResult> GetGroupsPagingAsync(string filter, int pageIndex, int pageSize)
        {
            var query = _db.Groups.AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(e => e.GroupName.Contains(filter));
            }
            var finalQuery =from g in query
                            join project in _db.Projects on g.ProjectId equals project.ProjectId into gProject
                            from project in gProject.DefaultIfEmpty()

                            join course in _db.Courses on g.CourseId equals course.CourseId into gCourse
                            from course in gCourse.DefaultIfEmpty()
                            select new GroupDto
                                {
                                GroupId = g.GroupId,
                                GroupName = g.GroupName,
                                ProjectId = project != null ? project.Title : "Chưa đăng ký",
                                CourseId = g.CourseId,
                                NumberOfMembers = g.NumberOfMembers,
                                CreatedAt = g.CreatedAt,
                                UpdatedAt = g.UpdatedAt,
                                DeletedAt = g.DeletedAt,
                                CourseGroup = course != null ? course.CourseGroup : "Không tìm thấy"
                            };
            var totalRecords = await finalQuery.CountAsync();
            var items = await finalQuery.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            var pagination = new Pagination<GroupDto>
            {
                Items = items,
                TotalRecords = totalRecords,
                PageSize = pageSize,
                PageIndex= pageIndex,
            };
            return Ok(new ApiResponse<Pagination<GroupDto>>(200,"Thành công", pagination));
        }
        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetByIdAsync(string groupId)
        {
            var query = from g in _db.Groups where g.GroupId == groupId
                        join project in _db.Projects on g.ProjectId equals project.ProjectId into gProject
                        from project in gProject.DefaultIfEmpty()

                        join course in _db.Courses on g.CourseId equals course.CourseId into gCourse
                        from course in gCourse.DefaultIfEmpty()
                        select new GroupDto
                        {
                            GroupId = g.GroupId,
                            GroupName = g.GroupName,
                            ProjectId = project != null ? project.Title : "Chưa đăng ký",
                            CourseId = g.CourseId,
                            NumberOfMembers = g.NumberOfMembers,
                            CreatedAt = g.CreatedAt,
                            UpdatedAt = g.UpdatedAt,
                            DeletedAt = g.DeletedAt,
                            CourseGroup = course != null ? course.CourseGroup : "Không tìm thấy"
                        };

            if (await query.CountAsync() == 0)
            {
                return NotFound(new ApiNotFoundResponse<string>("Không tìm thấy nhóm cần tìm"));
            }

            return Ok(new ApiResponse<GroupDto>(200,"Thành công", _mapper.Map<GroupDto>(await query.FirstOrDefaultAsync())));

        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostGroupAsync(CreateGroupRequestDto requestDto)
        {
            var groups = _db.Groups;
            if (await groups.AnyAsync(c => c.GroupName.Equals(requestDto.GroupName)))
            {
                return BadRequest(new ApiBadRequestResponse<string>("Tên nhóm không được trùng"));
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
            return Ok(new ApiResponse<GroupDto>(200,"Thành công", _mapper.Map<GroupDto>(newGroup)));
        }
        [HttpPut("{groupId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutGroupIdAsync(string groupId, [FromBody] CreateGroupRequestDto requestDto)
        {
            var group = await _db.Groups.FirstOrDefaultAsync(c => c.GroupId == groupId);
            if (group == null)
            {
                return NotFound(new ApiNotFoundResponse<string>($"Không tìm thấy nhóm với id : {groupId}"));
            }
            if (await _db.Groups.AnyAsync(e => e.GroupName == requestDto.GroupName && e.GroupId != groupId && e.CourseId == requestDto.CourseId))
            {
                return BadRequest(new ApiBadRequestResponse<string>("Tên nhóm không được trùng"));
            }

            group.GroupName = requestDto.GroupName; 
            group.ProjectId = requestDto.ProjectId;
            group.UpdatedAt = DateTime.Now;
            group.CourseId = requestDto.CourseId;

            _db.Groups.Update(group);
            var result = await _db.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(new ApiResponse<string>(200,"Thành công"));
            }
            return BadRequest(new ApiBadRequestResponse<string>("Cập nhật nhóm thất bại"));
        }

        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DeleteGroupAsync(string groupId)
        {
            var group = await _db.Groups.FirstOrDefaultAsync(c => c.GroupId == groupId);
            if (group == null)
            {
                return NotFound(new ApiNotFoundResponse<string>("Không thể tìm thấy nhóm với id"));
            }
            _db.Groups.Remove(group);
            var result = await _db.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(new ApiResponse<GroupDto>(200,"Thành công",_mapper.Map<GroupDto>(group)));
            }
            return BadRequest(new ApiBadRequestResponse<string>("Xóa thông tin nhóm thất bại"));
        }

        [HttpPost("{groupId}/members")]
        public async Task<IActionResult> PostAddMembersToGroupAsync(string groupId, AddMemberToGroupDto requestDto)
        {
            var group = await _db.Groups.FindAsync(groupId);
            if (group == null) 
            { 
                return NotFound(new ApiNotFoundResponse<string>("Không tìm thấy nhóm"));
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
            return Ok(new ApiResponse<string>(200,"Thêm thành công"));
        }
        [HttpDelete("{groupId}/members")]
        public async Task<IActionResult> DeleteRemoveMemberAsync(string groupId,RemoveMemberFromGroupDto requestDto)
        {
            var group = await _db.Groups.FindAsync(groupId);
            if (group == null) 
            { 
                return NotFound(new ApiNotFoundResponse<string>("Không tìm thấy nhóm"));
            } 
            if (requestDto.studentIds.Length > 0 ) { 
                foreach(var id in requestDto.studentIds)
                {
                    var student = await _userManager.FindByIdAsync(id);
                    if (student != null) 
                    {
                        var groupMember = await _db.GroupMembers.Where(c => c.GroupId == groupId && c.StudentId == id).FirstOrDefaultAsync();
                        if (groupMember != null) 
                        {
                            _db.GroupMembers.Remove(groupMember);
                        }
                    }
                }
                group.NumberOfMembers += requestDto.studentIds.Length;
            }
            await _db.SaveChangesAsync();
            return Ok(new ApiResponse<string>(200,"Thêm thành công"));
        }
        [HttpGet("{groupId}/members")]
        public async Task<IActionResult> GetGroupMembersAsync()
        {
            var data = from gr in _db.Groups
                       where gr.GroupId == gr.GroupId
                       join member in _db.GroupMembers on gr.GroupId equals member.GroupId
                       join user in _db.Users on member.StudentId equals user.Id
                       select new GroupMemberDto
                       {
                           GroupId = gr.GroupId,
                           IsLeader = member.IsLeader,  
                           StudentId = member.StudentId,
                           CreatedAt = member.CreatedAt,
                           DeletedAt = member.DeletedAt,
                           UpdatedAt = member.UpdatedAt,
                           StudentName = user.FullName
                       };
            return Ok(new ApiResponse<List<GroupMemberDto>>(200,"Thành công", await data.ToListAsync()));
        }

    }
}
