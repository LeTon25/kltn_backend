using AutoMapper;
using KLTN.Application.DTOs.Groups;
using KLTN.Application.DTOs.Reports;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Group = KLTN.Domain.Entities.Group;

namespace KLTN.Application.Services
{
    public class GroupService 
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IMapper mapper;
        private readonly ProjectService projectService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GroupService(IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            IMapper mapper,
            ProjectService projectService,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            this.mapper = mapper;
            this.projectService = projectService;
            _httpContextAccessor = httpContextAccessor;
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
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == requestDto.CourseId);
            if(userId == null)
            {
                return new ApiBadRequestResponse<object>("Người dùng hiện không đăng nhập");
            }    
            if(course == null)
            {
                return new ApiBadRequestResponse<object>("Không tìm thấy lớp học");

            }
            var newGroupId = Guid.NewGuid();
            var newGroup = new KLTN.Domain.Entities.Group()
            {
                GroupId = newGroupId.ToString(),
                GroupName = requestDto.GroupName,
                ProjectId = requestDto.ProjectId,
                NumberOfMembers = requestDto.NumberOfMembers,
                CourseId = requestDto.CourseId,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                
            };
            await _unitOfWork.GroupRepository.AddAsync(newGroup);

            if (userId != course.LecturerId)
            {
                await _unitOfWork.GroupMemberRepository.AddAsync(new GroupMember
                {
                    StudentId = userId,
                    IsLeader = true,
                    GroupId = newGroupId.ToString(),
                    CreatedAt = DateTime.Now
                });
            }    
            await _unitOfWork.SaveChangesAsync();
            var groupDto = await GetGroupDtoAsync(newGroupId.ToString());
            return new ApiResponse<object>(200, "Thêm nhóm thành công", mapper.Map<GroupDto>(groupDto));
        }
        public async Task<ApiResponse<object>> PutGroupAsync(string groupId,CreateGroupRequestDto requestDto,string currentUserId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c=>c.GroupId == groupId);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>($"Không tìm thấy nhóm với id : {groupId}");
            }
            var coures = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == group.CourseId);
           
            if (await _unitOfWork.GroupRepository.AnyAsync(e => e.GroupName == requestDto.GroupName && e.GroupId != groupId && e.CourseId == requestDto.CourseId))
            {
                return new ApiBadRequestResponse<object>("Tên nhóm không được trùng");
            }

            group.GroupName = requestDto.GroupName;
            group.ProjectId = requestDto.ProjectId;
            group.UpdatedAt = DateTime.Now;
            group.CourseId = requestDto.CourseId;
            group.NumberOfMembers = requestDto.NumberOfMembers;

            _unitOfWork.GroupRepository.Update(group);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                var groupDto = await GetGroupDtoAsync(groupId);
                return new ApiResponse<object>(200,"Thành công",groupDto);
            }
            return new ApiBadRequestResponse<object>("Cập nhật nhóm thất bại");
        }
        public async Task<ApiResponse<object>> DeleteGroupAsync(string groupId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId);
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
        public async Task<ApiResponse<object>> PostAddMembersToGroupAsync(string groupId, AddMemberToGroupDto requestDto,string userId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c=>c.GroupId==groupId,false,c=>c.Course,c => c.GroupMembers);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy nhóm");
            }
            var groupMemberByCurrentUser = await _unitOfWork.GroupMemberRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId && c.StudentId == userId);
            if ((groupMemberByCurrentUser != null && groupMemberByCurrentUser.IsLeader == false) || userId != group.Course?.LecturerId) 
            {
                return new ApiBadRequestResponse<object>("Bạn không có quyền thêm thành viên vào");
            }

            var groupMemberData = await _unitOfWork.GroupMemberRepository.GetAllAsync();
            var currentMembersCount = groupMemberData.Where(c=>c.GroupId == groupId).Count();

            if (requestDto.studentIds.Length > 0)
            {
                foreach (var id in requestDto.studentIds)
                {
                    var student = await _userManager.FindByIdAsync(id);

                    if (student != null)
                    {
                        if (!await _unitOfWork.EnrolledCourseRepository.AnyAsync(c => c.CourseId == group.CourseId && c.StudentId == student.Id))
                        {
                            return new ApiBadRequestResponse<object>("Có người dùng chưa tham gia lớp học");
                        }
                        if(!await CheckValidMemberAsync(student.Id, group))
                        {
                            return new ApiBadRequestResponse<object>("Có người dùng đã tham gia nhóm khác");
                        }
                        if(group.GroupMembers != null && group.GroupMembers.Any(c => c.StudentId.Equals(id)))
                        {
                            return new ApiBadRequestResponse<object>("Có người dùng đã tham gia rồi");
                        }    
       
                        if (currentMembersCount + 1 > group.NumberOfMembers)
                        {
                            return new ApiBadRequestResponse<object>("Số lượng thành viên vượt quá số lượng cho phép");
                        }    
                        await _unitOfWork.GroupMemberRepository.AddAsync(new GroupMember()
                        {
                            GroupId = groupId,
                            StudentId = student.Id,
                            IsLeader = false,
                        });
                    }
                }
            }
            await _unitOfWork.SaveChangesAsync();
            var groupMemberDto = await GetGroupMemberDtoAsync(groupId);
            return new ApiSuccessResponse<object>(200, "Thêm thành viên thành công", groupMemberDto);
        }
        public async Task<ApiResponse<object>> DeleteRemoveMemberAsync(string groupId, RemoveMemberFromGroupDto requestDto,string userId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId,false,c=>c.Course);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy nhóm");
            }
            var groupMemberByCurrentUser = await _unitOfWork.GroupMemberRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId && c.StudentId == userId);
            if ((groupMemberByCurrentUser != null && groupMemberByCurrentUser.IsLeader == false) || userId != group.Course?.LecturerId)
            {
                return new ApiBadRequestResponse<object>("Bạn không có quyền xóa thành viên");
            }
            if (requestDto.studentIds.Length > 0)
            {
                foreach (var id in requestDto.studentIds)
                {
                    var student = await _userManager.FindByIdAsync(id);
                    if (student != null)
                    {
                        var groupMember = await _unitOfWork.GroupMemberRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId && c.StudentId == id);
                        if (groupMember != null && !groupMember.IsLeader)
                        {
                            _unitOfWork.GroupMemberRepository.Delete(groupMember);
                        }
                    }
                }
            }
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Xóa thành công");
        }
        public async Task<ApiResponse<object>> AssignLeaderAsync(string currentUserId, string groupId , string leaderId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId, false, c => c.Course);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy nhóm");
            }
            var groupMemberByCurrentUser = await _unitOfWork.GroupMemberRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId && c.StudentId == currentUserId);
            if ((groupMemberByCurrentUser != null && groupMemberByCurrentUser.IsLeader == false) || currentUserId != group.Course?.LecturerId)
            {
                return new ApiBadRequestResponse<object>("Bạn không có quyền xóa thành viên");
            }
            var oldLeader = await _unitOfWork.GroupMemberRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId && c.IsLeader == true);
            if(oldLeader != null)
            {
                oldLeader.IsLeader = false;
                _unitOfWork.GroupMemberRepository.Update(oldLeader);
            }    

            var newLeader = await _unitOfWork.GroupMemberRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId && c.StudentId == leaderId);
            if (newLeader == null) { 
                return new ApiBadRequestResponse<object>("Người bạn chọn làm nhóm trưởng vẫn chưa tham gia nhóm");
            }
            newLeader.IsLeader = true;
            _unitOfWork.GroupMemberRepository.Update(newLeader);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Thêm thành công");
        }

        #endregion
        public async Task<GroupDto?> GetGroupDtoAsync(string groupId )
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId);
            if (group == null) 
            {
                return null;
            }

            var groupDto = mapper.Map<GroupDto>(group);
            if(groupDto.ProjectId != null)
            {
                groupDto.Project = await projectService.GetProjectDtoAsync(groupDto.ProjectId);
            }    
            groupDto.GroupMembers = await GetGroupMemberDtoAsync(groupId);

            return groupDto;
        }
        public async Task<bool> CheckValidMemberAsync(string studentId,Group group)
        {
            var allGroup = await _unitOfWork.GroupRepository.GetAllAsync();
            var groupsInCourse = allGroup.Where(c => c.CourseId == group.CourseId).ToList();
            
            foreach(var gr in groupsInCourse)
            {
                var checkExisted = await _unitOfWork.GroupMemberRepository.GetFirstOrDefaultAsync(c=>c.GroupId == gr.GroupId && c.StudentId ==studentId );
                if (checkExisted != null) {
                    return false;
                }
            }   
            return true;
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
