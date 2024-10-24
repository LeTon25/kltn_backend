﻿using AutoMapper;
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
using System.Net.WebSockets;
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
        public async Task<ApiResponse<object>> GetByIdAsync(string Id)
        {
            var groupDto = await GetGroupDtoAsync(Id);
            if (groupDto == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy nhóm", groupDto);
            }
            return new ApiSuccessResponse<object>(200, "Lấy dữ liệu thành công", groupDto);
        }
        public async Task<ApiResponse<object>> PostGroupAsync(CreateGroupRequestDto requestDto)
        {
            if (await _unitOfWork.GroupRepository.AnyAsync(c => c.GroupName == requestDto.GroupName && c.CourseId == requestDto.CourseId))
            {
                return new ApiBadRequestResponse<object>("Tên nhóm đã tồn tại");
            }
            var userId = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == requestDto.CourseId,false,c=>c.Setting!);
            if (course == null)
            {
                return new ApiBadRequestResponse<object>("Không tìm thấy lớp học");

            }
            if (course.LecturerId != userId) 
            {
                if (!course.Setting!.AllowGroupRegistration)
                {
                    return new ApiBadRequestResponse<object>("Giảng viên chưa cho phép SV đăng ký nhóm");
                }
                var now = DateTime.Now;
                if(course.Setting.StartGroupCreation != null && course.Setting.EndGroupCreation != null)
                {
                    if (now < course.Setting.StartGroupCreation || now > course.Setting.EndGroupCreation)
                    { 
                        return new ApiBadRequestResponse<object>("Qua thòi gian đăng ký nhóm.Vui lòng liên hệ giảng viên");
                    }
                }
                var groups = await _unitOfWork.GroupRepository.FindByCondition(c => c.CourseId.Equals(course.CourseId),false,c=>c.GroupMembers).ToListAsync();
                if (groups.Any(c=>c.GroupMembers.Any(e=>e.StudentId.Equals(userId))))
                {
                    return new ApiBadRequestResponse<object>("Bạn đã ở trong nhóm khác");
                }
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
                    StudentId = userId!,
                    IsLeader = true,
                    GroupId = newGroupId.ToString(),
                    CreatedAt = DateTime.Now
                });
            }
            await _unitOfWork.SaveChangesAsync();
            var groupDto = await GetGroupDtoAsync(newGroupId.ToString());
            return new ApiResponse<object>(200, "Thêm nhóm thành công", mapper.Map<GroupDto>(groupDto));
        }
        public async Task<ApiResponse<object>> PutGroupAsync(string groupId, CreateGroupRequestDto requestDto, string currentUserId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>($"Không tìm thấy nhóm với id : {groupId}");
            }
            var coures = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == group.CourseId);

            if (await _unitOfWork.GroupRepository.AnyAsync(e => e.GroupName == requestDto.GroupName && e.GroupId != groupId && e.CourseId == requestDto.CourseId))
            {
                return new ApiBadRequestResponse<object>("Tên nhóm không được trùng");
            }
            if (group.IsApproved != requestDto.IsApproved)
            {
                if (currentUserId != coures.LecturerId)
                {
                    return new ApiBadRequestResponse<object>("Chỉ giáo viên mới có quyền duyệt nhóm");
                }
                group.IsApproved = requestDto.IsApproved;
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
                return new ApiResponse<object>(200, "Thành công", groupDto);
            }
            return new ApiBadRequestResponse<object>("Cập nhật nhóm thất bại");
        }
        public async Task<ApiResponse<object>> DeleteGroupAsync(string groupId)
        {
            string currentUserId = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId, false, c => c.GroupMembers, c => c.Course!);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>("Không thể tìm thấy nhóm với id");
            }
            var isLeader = group.GroupMembers.Any(c => c.StudentId == currentUserId);
            var isTeacher = group.Course!.LecturerId == currentUserId;
            if (!isLeader && !isTeacher)
            {
                return new ApiBadRequestResponse<object>("Bạn không có quyền xóa nhóm");
            }
            var groupMembers = _unitOfWork.GroupMemberRepository.GetAll(c => c.GroupId == groupId);
            _unitOfWork.GroupRepository.Delete(group);
            _unitOfWork.GroupMemberRepository.DeleteRange(groupMembers);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Thành công", mapper.Map<GroupDto>(group));
        }
        public async Task<ApiResponse<object>> PostAddMembersToGroupAsync(string groupId, AddMemberToGroupDto requestDto, string userId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId, false, c => c.Course!);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy nhóm");
            }
            var groupMembers = await _unitOfWork.GroupMemberRepository.FindByCondition(c => c.GroupId.Equals(groupId), false, c => c.Member!).ToListAsync();

            var groupMemberByCurrentUser = groupMembers.Where(c => c.GroupId == groupId && c.StudentId == userId).FirstOrDefault();
            if ((groupMemberByCurrentUser != null && groupMemberByCurrentUser.IsLeader == false) && userId != group.Course?.LecturerId)
            {
                return new ApiBadRequestResponse<object>("Bạn không có quyền thêm thành viên vào");
            }

            var currentMembersCount = groupMembers.Count;

            if (requestDto.Emails.Length > 0)
            {
                foreach (var email in requestDto.Emails)
                {
                    var student = await _userManager.FindByEmailAsync(email);

                    if (student != null)
                    {
                        if (!await _unitOfWork.EnrolledCourseRepository.AnyAsync(c => c.CourseId == group.CourseId && c.StudentId == student.Id))
                        {
                            return new ApiBadRequestResponse<object>("Có người dùng chưa tham gia lớp học");
                        }
                        if (!await CheckValidMemberAsync(student.Id, group))
                        {
                            return new ApiBadRequestResponse<object>("Có người dùng đã tham gia nhóm khác");
                        }
                        if (group.GroupMembers != null && group.GroupMembers.Any(c => c.StudentId.Equals(student.Id)))
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
                        currentMembersCount += 1;
                    }
                }
            }
            await _unitOfWork.SaveChangesAsync();
            var groupMemberDto = await GetGroupMemberDtoAsync(groupId);
            return new ApiSuccessResponse<object>(200, "Thêm thành viên thành công", groupMemberDto);
        }
        public async Task<ApiResponse<object>> DeleteRemoveMemberAsync(string groupId, RemoveMemberFromGroupDto requestDto, string userId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId, false, c => c.Course!, c => c.GroupMembers);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy nhóm");
            }
            var groupMembers = await _unitOfWork.GroupMemberRepository.FindByCondition(c => c.GroupId.Equals(groupId), false, c => c.Member!).ToListAsync();

            var groupMemberByCurrentUser = groupMembers.Where(c => c.GroupId == groupId && c.StudentId == userId).FirstOrDefault();
            if ((groupMemberByCurrentUser != null && groupMemberByCurrentUser.IsLeader == false) && userId != group.Course?.LecturerId)
            {
                return new ApiBadRequestResponse<object>("Bạn không có quyền xóa thành viên");
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
            return new ApiResponse<object>(200, "Xóa thành công");
        }
        public async Task<ApiResponse<object>> AssignLeaderAsync(string currentUserId, string groupId, string leaderId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId, false, c => c.Course!);
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
            if (oldLeader != null)
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
        public async Task<ApiResponse<object>> GetRegenerateInviteCodeAsync(string groupId)
        {
            var currentUserId = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId.Equals(groupId), false, c => c.Course!, c => c.GroupMembers);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy nhóm");
            }
            var groupMemberByCurrentUser = await _unitOfWork.GroupMemberRepository.GetFirstOrDefaultAsync(c => c.GroupId == groupId && c.StudentId == currentUserId);
            if ((groupMemberByCurrentUser != null && groupMemberByCurrentUser.IsLeader == false) || currentUserId != group.Course?.LecturerId)
            {
                return new ApiBadRequestResponse<object>("Bạn không có quyền tạo mã mời");
            }
            group.InviteCode = GenerateRandomNumericString(6);
            group.InviteCodeExpired = DateTime.Now.AddMinutes(5);
            _unitOfWork.GroupRepository.Update(group);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Tạo mã mời thành công", new GroupInviteCodeDto
            {
                InviteCode = group.InviteCode,
                InviteCodeExpired = group.InviteCodeExpired
            });
        }
        public async Task<ApiResponse<object>> ApplyCodeAsync(JoinGroupDto requestDto, string groupId)
        {
            var currentUserId = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId.Equals(groupId), false, c => c.Course!, c => c.GroupMembers);
            if (group == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy nhóm");
            }
            if (currentUserId == group.Course!.LecturerId)
            {
                return new ApiBadRequestResponse<object>("Giáo viên của lớp không thể tham gia nhóm");
            }
            if (!await _unitOfWork.EnrolledCourseRepository.AnyAsync(c => c.CourseId == group.CourseId && c.StudentId == currentUserId))
            {
                return new ApiBadRequestResponse<object>("Bạn chưa tham gia lớp học");
            }
            if (!await CheckValidMemberAsync(currentUserId!, group))
            {
                return new ApiBadRequestResponse<object>("Bạn đã tham gia nhóm khác");
            }
            if (group.GroupMembers != null && group.GroupMembers.Any(c => c.StudentId.Equals(currentUserId)))
            {
                return new ApiBadRequestResponse<object>("Bạn đã tham gia nhóm rồi");
            }

            var currentDate = DateTime.Now;
            if (requestDto.InviteCode == group.InviteCode && currentDate < group.InviteCodeExpired)
            {
                var currentMemberCount = group.GroupMembers.Count();
                if (currentMemberCount + 1 > group.NumberOfMembers)
                {
                    return new ApiNotFoundResponse<object>("Nhóm đã đủ số lượng thành viên");
                }
                await _unitOfWork.GroupMemberRepository.AddAsync(new GroupMember()
                {
                    GroupId = groupId,
                    StudentId = currentUserId!,
                    IsLeader = false,
                });
                await _unitOfWork.SaveChangesAsync();
                return await GetByIdAsync(groupId);
            }
            else
            {
                return new ApiBadRequestResponse<object>("Mã mời không hợp lệ");
            }
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
        private string GenerateRandomNumericString(int length)
        {
            Random random = new Random();
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = (char)('0' + random.Next(0, 10));
            }

            return new string(result);
        }
    }
}
