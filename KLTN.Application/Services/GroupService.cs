using AutoMapper;
using AutoMapper.Execution;
using KLTN.Application.DTOs.Groups;
using KLTN.Application.DTOs.Projects;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KLTN.Application.Services
{
    public class GroupService 
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IMapper mapper;
        private readonly ProjectService projectService;
        public GroupService(IUnitOfWork unitOfWork, UserManager<User> userManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            this.mapper = mapper;
        }
        #region for controller
        public async Task<ApiResponse<object>> GetByIdAsync(string Id)
        {
            var groupDto = await GetGroupDtoAsync(Id);
            if(groupDto == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy nhóm",groupDto);
            }
            return new ApiSuccessResponse<object>(200, "Lấy dữ liệu thành công", groupDto);
        }
        public async Task<ApiResponse<object>> PostGroupAsync(CreateGroupRequestDto requestDto)
        {
            if(await _unitOfWork.GroupRepository.AnyAsync(c=>c.GroupName == requestDto.GroupName && c.CourseId == requestDto.CourseId))
            {
                return new ApiBadRequestResponse<object>("Tên nhóm đã tồn tại");
            }
            var newGroupId = Guid.NewGuid();
            var newGroup = new KLTN.Domain.Entities.Group()
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
            await _unitOfWork.GroupRepository.AddAsync(newGroup);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Thêm nhóm thành công", mapper.Map<GroupDto>(newGroup));
        }
        public async Task<ApiResponse<object>> PutGroupAsync(string groupId,CreateGroupRequestDto requestDto)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefault(c=>c.GroupId == groupId);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>($"Không tìm thấy nhóm với id : {groupId}");
            }
            if (await _unitOfWork.GroupRepository.AnyAsync(e => e.GroupName == requestDto.GroupName && e.GroupId != groupId && e.CourseId == requestDto.CourseId))
            {
                return new ApiBadRequestResponse<object>("Tên nhóm không được trùng");
            }

            group.GroupName = requestDto.GroupName;
            group.ProjectId = requestDto.ProjectId;
            group.UpdatedAt = DateTime.Now;
            group.CourseId = requestDto.CourseId;

            _unitOfWork.GroupRepository.Update(group);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Thành công");
            }
            return new ApiBadRequestResponse<object>("Cập nhật nhóm thất bại");
        }
        public async Task<ApiResponse<object>> DeleteGroupAsync(string groupId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefault(c => c.GroupId == groupId);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>("Không thể tìm thấy nhóm với id");
            }
            var groupMembers = _unitOfWork.GroupMemberRepository.GetAll(c => c.GroupId == groupId);
            _unitOfWork.GroupRepository.Delete(group);
            _unitOfWork.GroupMemberRepository.DeleteRange(groupMembers);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Thành công", mapper.Map<GroupDto>(group));
        }
        public async Task<ApiResponse<object>> PostAddMembersToGroupAsync(string groupId, AddMemberToGroupDto requestDto)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefault(c=>c.GroupId==groupId);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy nhóm");
            }
            if (requestDto.studentIds.Length > 0 && !string.IsNullOrEmpty(requestDto.leaderId))
            {
                foreach (var id in requestDto.studentIds)
                {
                    var student = await _userManager.FindByIdAsync(id);
                    if (student != null)
                    {
                        await _unitOfWork.GroupMemberRepository.AddAsync(new GroupMember()
                        {
                            GroupId = groupId,
                            StudentId = student.Id,
                            IsLeader = student.Id == requestDto.leaderId,
                        });
                    }
                }
                group.NumberOfMembers += requestDto.studentIds.Length;
            }
            await _unitOfWork.SaveChangesAsync();
            var groupMemberDto = await GetGroupMemberDtoAsync(groupId);
            return new ApiSuccessResponse<object>(200, "Thêm thành viên thành công", groupMemberDto);
        }

        public async Task<ApiResponse<object>> DeleteRemoveMemberAsync(string groupId, RemoveMemberFromGroupDto requestDto)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefault(c => c.GroupId == groupId);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy nhóm");
            }
            if (requestDto.studentIds.Length > 0)
            {
                foreach (var id in requestDto.studentIds)
                {
                    var student = await _userManager.FindByIdAsync(id);
                    if (student != null)
                    {
                        var groupMember = await _unitOfWork.GroupMemberRepository.GetFirstOrDefault(c => c.GroupId == groupId && c.StudentId == id);
                        if (groupMember != null)
                        {
                            _unitOfWork.GroupMemberRepository.Delete(groupMember);
                        }
                    }
                }
                group.NumberOfMembers -= requestDto.studentIds.Length;
            }
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Thêm thành công");
        }
        #endregion
        public async Task<GroupDto?> GetGroupDtoAsync(string groupId )
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefault(c => c.GroupId == groupId);
            if (group == null) 
            {
                return null;
            }
            var groupDto = mapper.Map<GroupDto>(group);
            groupDto.Project = await projectService.GetProjectDtoAsync(groupDto.ProjectId);
            groupDto.GroupMembers = await GetGroupMemberDtoAsync(groupId);

            return groupDto;
        }

        public async Task<List<GroupMemberDto>?> GetGroupMemberDtoAsync(string groupId)
        {
            var groupMembersData = _unitOfWork.GroupMemberRepository.GetAll(c => c.GroupId == groupId);
            var userIds = groupMembersData.Select(c => c.StudentId).ToList();
            var users =  await _userManager.Users.Where(c=> userIds.Contains(c.Id)).ToListAsync();
            var groupMemberDto = new List<GroupMemberDto>();

            foreach(var groupMember in groupMembersData)
            {
                groupMemberDto.Add(new GroupMemberDto()
                {
                    GroupId = groupMember.GroupId,
                    IsLeader = groupMember.IsLeader,
                    StudentId = groupMember.StudentId,
                    CreatedAt = groupMember.CreatedAt,
                    DeletedAt = groupMember.DeletedAt,
                    UpdatedAt = groupMember.UpdatedAt,
                    StudentObj = mapper.Map<UserDto>(users.FirstOrDefault(c=>c.Id == groupMember.StudentId))
                });
            }
            return groupMemberDto;
        }
    }
}
