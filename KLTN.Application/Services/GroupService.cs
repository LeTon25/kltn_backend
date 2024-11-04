using AutoMapper;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Groups;
using KLTN.Application.DTOs.Reports;
using KLTN.Application.DTOs.Requests;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Enums;
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
        private readonly CommentService commentService;
        public GroupService(IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            IMapper mapper,
            ProjectService projectService,
            CommentService commentService,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            this.mapper = mapper;
            this.projectService = projectService;
            this.commentService = commentService;
            _httpContextAccessor = httpContextAccessor;
        }
        #region for controller
        public async Task<ApiResponse<GroupDto>> GetByIdAsync(string Id)
        {
            var groupDto = await GetGroupDtoAsync(Id);
            if (groupDto == null)
            {
                return new ApiNotFoundResponse<GroupDto>("Không tìm thấy nhóm", groupDto);
            }
            return new ApiSuccessResponse<GroupDto>(200, "Lấy dữ liệu thành công", groupDto);
        }
        public async Task<ApiResponse<GroupDto>> PostGroupAsync(CreateGroupRequestDto requestDto)
        {
            if (await _unitOfWork.GroupRepository.AnyAsync(c => c.GroupName == requestDto.GroupName && c.CourseId == requestDto.CourseId))
            {
                return new ApiBadRequestResponse<GroupDto>("Tên nhóm đã tồn tại");
            }
            var userId = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == requestDto.CourseId,false,c=>c.Setting!);
            if (course == null)
            {
                return new ApiBadRequestResponse<GroupDto>("Không tìm thấy lớp học");

            }
            if (course.LecturerId != userId) 
            {
                return new ApiBadRequestResponse<GroupDto>("Chỉ giáo viên mới có quyền tạo nhóm");
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
                IsApproved = true,
            };
            await _unitOfWork.GroupRepository.AddAsync(newGroup);

            await _unitOfWork.SaveChangesAsync();
            var groupDto = await GetGroupDtoAsync(newGroupId.ToString());
            return new ApiResponse<GroupDto>(200, "Thêm nhóm thành công", mapper.Map<GroupDto>(groupDto));
        }
        public async Task<ApiResponse<GroupDto>> PutGroupAsync(string groupId, CreateGroupRequestDto requestDto, string currentUserId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId,false,c=>c.Course!);
            if (group == null)
            {
                return new ApiNotFoundResponse<GroupDto>($"Không tìm thấy nhóm với id : {groupId}");
            }

            if (await _unitOfWork.GroupRepository.AnyAsync(e => e.GroupName == requestDto.GroupName && e.GroupId != groupId && e.CourseId == group.CourseId))
            {
                return new ApiBadRequestResponse<GroupDto>("Tên nhóm không được trùng");
            }
            if(currentUserId != group.Course!.LecturerId)
            {
                return new ApiBadRequestResponse<GroupDto>("Chỉ giáo viên mới có quyền chỉnh sửa nhóm");
            }
            group.GroupName = requestDto.GroupName;
            group.ProjectId = requestDto.ProjectId;
            group.UpdatedAt = DateTime.Now;
            group.NumberOfMembers = requestDto.NumberOfMembers;
            _unitOfWork.GroupRepository.Update(group);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                var groupDto = await GetGroupDtoAsync(groupId);
                return new ApiResponse<GroupDto>(200, "Thành công", groupDto);
            }
            return new ApiBadRequestResponse<GroupDto>("Cập nhật nhóm thất bại");
        }
        public async Task<ApiResponse<GroupDto>> DeleteGroupAsync(string groupId)
        {
            string currentUserId = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId, false, c => c.GroupMembers, c => c.Course!);
            if (group == null)
            {
                return new ApiNotFoundResponse<GroupDto>("Không thể tìm thấy nhóm với id");
            }
            var isTeacher = group.Course!.LecturerId == currentUserId;
            if (!isTeacher)
            {
                return new ApiBadRequestResponse<GroupDto>("Bạn không có quyền xóa nhóm");
            }
            var groupMembers = _unitOfWork.GroupMemberRepository.GetAll(c => c.GroupId == groupId);
            
            _unitOfWork.GroupRepository.Delete(group);
            _unitOfWork.GroupMemberRepository.DeleteRange(groupMembers);

            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<GroupDto>(200, "Thành công", mapper.Map<GroupDto>(group));
        }
        public async Task<ApiResponse<List<GroupMemberDto>>> PostAddMembersToGroupAsync(string groupId, AddMemberToGroupDto requestDto, string userId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId, false, c => c.Course!);
            if (group == null)
            {
                return new ApiNotFoundResponse<List<GroupMemberDto>>("Không tìm thấy nhóm");
            }
            var groupMembers = await _unitOfWork.GroupMemberRepository.FindByCondition(c => c.GroupId.Equals(groupId), false, c => c.Member!).ToListAsync();

            var groupMemberByCurrentUser = groupMembers.Where(c => c.GroupId == groupId && c.StudentId == userId).FirstOrDefault();
            if ((groupMemberByCurrentUser != null && groupMemberByCurrentUser.IsLeader == false) && userId != group.Course?.LecturerId)
            {
                return new ApiBadRequestResponse<List<GroupMemberDto>>("Bạn không có quyền thêm thành viên vào");
            }

            var currentMembersCount = groupMembers != null ?  groupMembers.Count : 0;

            if (requestDto.Emails.Length > 0)
            {
                foreach (var email in requestDto.Emails)
                {
                    var student = await _userManager.FindByEmailAsync(email);

                    if (student != null)
                    {
                        if (!await _unitOfWork.EnrolledCourseRepository.AnyAsync(c => c.CourseId == group.CourseId && c.StudentId == student.Id))
                        {
                            return new ApiBadRequestResponse<List<GroupMemberDto>>("Có người dùng chưa tham gia lớp học");
                        }
                        if (!await CheckValidMemberAsync(student.Id, group))
                        {
                            return new ApiBadRequestResponse<List<GroupMemberDto>>("Có người dùng đã tham gia nhóm khác");
                        }
                        if (group.GroupMembers != null && group.GroupMembers.Any(c => c.StudentId.Equals(student.Id)))
                        {
                            return new ApiBadRequestResponse<List<GroupMemberDto>>("Có người dùng đã tham gia rồi");
                        }

                        if (currentMembersCount + 1 > group.NumberOfMembers)
                        {
                            return new ApiBadRequestResponse<List<GroupMemberDto>>("Số lượng thành viên vượt quá số lượng cho phép");
                        }
                        await _unitOfWork.GroupMemberRepository.AddAsync(new GroupMember()
                        {
                            GroupId = groupId,
                            StudentId = student.Id,
                            IsLeader = currentMembersCount == 0 ? true : false,
                        });
                        currentMembersCount += 1;
                    }
                }
            }
            await _unitOfWork.SaveChangesAsync();
            var groupMemberDto = await GetGroupMemberDtoAsync(groupId);
            return new ApiSuccessResponse<List<GroupMemberDto>>(200, "Thêm thành viên thành công", groupMemberDto!);
        }
        public async Task<ApiResponse<List<GroupMemberDto>>> DeleteRemoveMemberAsync(string groupId, RemoveMemberFromGroupDto requestDto, string userId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId, false, c => c.Course!, c => c.GroupMembers);
            if (group == null)
            {
                return new ApiNotFoundResponse<List<GroupMemberDto>>("Không tìm thấy nhóm");
            }
            var groupMembers = await _unitOfWork.GroupMemberRepository.FindByCondition(c => c.GroupId.Equals(groupId), false, c => c.Member!).ToListAsync();

            var groupMemberByCurrentUser = groupMembers.Where(c => c.GroupId == groupId && c.StudentId == userId).FirstOrDefault();
            if ((groupMemberByCurrentUser != null && groupMemberByCurrentUser.IsLeader == false) && userId != group.Course?.LecturerId)
            {
                return new ApiBadRequestResponse<List<GroupMemberDto>>("Bạn không có quyền xóa thành viên");
            }
            if (requestDto.studentIds.Length > 0)
            {
                foreach (var id in requestDto.studentIds)
                {
                        var groupMember = groupMembers.Where(c => c.GroupId == groupId && c.StudentId == id).FirstOrDefault();
                        if (groupMember != null && !groupMember.IsLeader)
                        {
                            _unitOfWork.GroupMemberRepository.Delete(groupMember);
                        }
                }
            }
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<List<GroupMemberDto>>(200, "Xóa thành công");
        }
        public async Task<ApiResponse<object>> AssignLeaderAsync(string currentUserId, string groupId, string leaderId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId, false, c => c.Course!,c => c.GroupMembers);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy nhóm");
            }
            var groupMemberByCurrentUser = group.GroupMembers != null ? group.GroupMembers.FirstOrDefault(c => c.GroupId == groupId && c.StudentId == currentUserId) : null;
            if ((groupMemberByCurrentUser != null && groupMemberByCurrentUser.IsLeader == false) || currentUserId != group.Course?.LecturerId)
            {
                return new ApiBadRequestResponse<object>("Bạn không có quyền xóa thành viên");
            }
            var oldLeader = group.GroupMembers != null ? group.GroupMembers.FirstOrDefault(c => c.GroupId == groupId && c.IsLeader == true) : null;
            if (oldLeader != null)
            {
                oldLeader.IsLeader = false;
                _unitOfWork.GroupMemberRepository.Update(oldLeader);
            }

            var newLeader = group.GroupMembers != null ? group.GroupMembers.FirstOrDefault(c => c.GroupId == groupId && c.StudentId == leaderId) : null;
            if (newLeader == null) {
                return new ApiBadRequestResponse<object>("Người bạn chọn làm nhóm trưởng vẫn chưa tham gia nhóm");
            }
            newLeader.IsLeader = true;
            _unitOfWork.GroupMemberRepository.Update(newLeader);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Thêm thành công");
        }
        public async Task<ApiResponse<object>> GetReportsInGroupAsync(string groupId)
        {
            var reports = await _unitOfWork.ReportRepository.FindByCondition(c => c.GroupId.Equals(groupId), false, c => c.CreateUser!).ToListAsync();
            var reportDtos = mapper.Map<List<ReportDto>>(reports);
            foreach (var reportDto in reportDtos)
            {
                reportDto.Comments = await commentService.GetCommentDtosFromPostAsync(reportDto.ReportId, CommentableType.Report);

            }
            var sortedReportDtos = reportDtos
                    .OrderByDescending(c => c.CreatedAt)
                    .ThenByDescending(c => c.UpdatedAt)
                    .ToList();
            return new ApiResponse<object>(200, "Lấy dữ liệu thành công", sortedReportDtos);
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
            var requests = await _unitOfWork.RequestRepository.FindByCondition(c => c.GroupId.Equals(groupId), false, c => c.User!).ToListAsync();
            groupDto.Requests = mapper.Map<List<RequestDto>>(requests);
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
            var groupMembersData = await _unitOfWork.GroupMemberRepository.FindByCondition(c => c.GroupId == groupId,false,c=>c.Member!).ToListAsync();
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
                    StudentObj = mapper.Map<UserDto>(groupMember.Member)
                });
            }
            return groupMemberDto;
        }
        public async Task<ApiResponse<List<GroupDto>>> AutoGenerateGroupAsync(string courseId,AutoGenerateGroupDto requestDto,string currentUserId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId.Equals(courseId), false,c=>c.Setting!);
            
            if(course == null)
            {
                return new ApiBadRequestResponse<List<GroupDto>>("Không tìm thấy lớp học");
            }
            if(currentUserId != course.LecturerId)
            {
                return new ApiResponse<List<GroupDto>>(403,"Chỉ có giáo viên mới có quyền tạo nhóm tự động");
            }    
            var allGroupsInCourse = await _unitOfWork.GroupRepository.FindByCondition(c=>c.CourseId.Equals(courseId),false).ToListAsync();
            var totalGroup = allGroupsInCourse.Count;
            var newGroupsAdded = new List<GroupDto>();
            for (int i = 0 ; i < requestDto.Count;i++)
            {
                var index = totalGroup + i + 1;
                var newGroupName = $"Nhóm {index}";

                while (allGroupsInCourse.Any(e=>e.GroupName.Equals(newGroupName)))
                {
                    index++;
                    newGroupName = $"Nhóm {index}";
                }
                var newGroup = new KLTN.Domain.Entities.Group()
                {
                    GroupId = Guid.NewGuid().ToString(),
                    GroupName = newGroupName,
                    ProjectId = null,
                    NumberOfMembers = course.Setting!.MaxGroupSize,
                    CourseId = courseId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = null,
                    DeletedAt = null,
                    IsApproved = true,
                };
                await _unitOfWork.GroupRepository.AddAsync(newGroup);
                allGroupsInCourse.Add(newGroup);
                newGroupsAdded.Add( mapper.Map<GroupDto>(newGroup));
                
            }
            await _unitOfWork.SaveChangesAsync();
            foreach (var group in newGroupsAdded) 
            {
                group.Course = mapper.Map<CourseDto>(course);
                group.GroupMembers = new List<GroupMemberDto>();
            }
            return new ApiResponse<List<GroupDto>>(200,"Thêm thành công",newGroupsAdded);
        }
    }
}
