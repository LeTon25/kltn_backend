using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KLTN.Domain.Entities;
using AutoMapper;
using KLTN.Application.Helpers.Response;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Users;
using KLTN.Application.DTOs.Subjects;
using KLTN.Application.DTOs.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using KLTN.Application.DTOs.Groups;
using KLTN.Application.DTOs.Assignments;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using KLTN.Domain.Util;
using KLTN.Application.DTOs.ScoreStructures;
namespace KLTN.Application.Services
{
    public class CourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IMapper mapper;
        private readonly AnnoucementService annoucementService;
        private readonly GroupService groupService;
        private readonly IHttpContextAccessor httpContextAccessor;
        public CourseService(IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            IMapper mapper,
            AnnoucementService annoucementService,
            GroupService groupService,
            IHttpContextAccessor httpContextAccessor) 
        { 
            this._unitOfWork = unitOfWork;
            this._userManager = userManager;
            this.mapper = mapper;  
            this.annoucementService = annoucementService;
            this.groupService = groupService;
            this.httpContextAccessor = httpContextAccessor;
        }
        #region for controller
        public async Task<ApiResponse<object>> GetAllCoursesAsync()
        {
            var courses = await _unitOfWork.CourseRepository.GetAllAsync();
            var courseDtos = mapper.Map<List<CourseDto>>(courses.ToList());
            foreach(var courseDto in courseDtos)
            {
                var lecturer = mapper.Map<UserDto>(await _userManager.FindByIdAsync(courseDto.LecturerId)) ;
                var subject = mapper.Map<SubjectDto>(await _unitOfWork.SubjectRepository.GetFirstOrDefaultAsync(c=>c.SubjectId == courseDto.SubjectId));
                courseDto.Lecturer = lecturer;
                courseDto.Subject = subject;
            }
            return new ApiResponse<object>(200, "Thành công", courseDtos);
        }
        public async Task<ApiResponse<object>> UpdateInviteCodeAsync(string courseId, string inviteCode)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy lớp");
            }
            if (string.IsNullOrEmpty(inviteCode))
            {
                return new ApiBadRequestResponse<object>("Mã mời không được trống");
            }
            if (await _unitOfWork.CourseRepository.AnyAsync(c => c.CourseId != courseId && c.InviteCode == inviteCode))
            {
                return new ApiBadRequestResponse<object>("Mã mời không được trùng");
            }
            course.InviteCode = inviteCode;
            _unitOfWork.CourseRepository.Update(course);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Cập nhật mã mời thành công", mapper.Map<CourseDto>(course));
            }
            return new ApiBadRequestResponse<object>("Cập nhật mã mời thất bại");
        }
        public async Task<ApiResponse<object>> CreateCourseAsync(CreateCourseRequestDto requestDto)
        {
            var currentUserId = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (await _unitOfWork.CourseRepository.AnyAsync(c => c.CourseGroup == requestDto.CourseGroup && c.LecturerId == currentUserId  && c.SubjectId == requestDto.SubjectId))
            {
                return new ApiBadRequestResponse<object>("Nhóm môn học được mở không được trùng");
            }
            if (await _unitOfWork.CourseRepository.AnyAsync(c => c.InviteCode == requestDto.InviteCode))
            {
                return new ApiBadRequestResponse<object>("Mã mời không được trùng");
            }
            var newCourseId = Guid.NewGuid();
            var newCourse = new Course()
            {
                CourseId = newCourseId.ToString(),
                CourseGroup = requestDto.CourseGroup,
                EnableInvite = requestDto.EnableInvite,
                InviteCode = requestDto.InviteCode ?? GenerateRandomNumericString(6),
                LecturerId = currentUserId,
                SubjectId = requestDto.SubjectId,
                CreatedAt = DateTime.Now,
                Semester = requestDto.Semester,
                UpdatedAt = null,
                DeletedAt = null,
                Name = requestDto.Name,
            };
            await _unitOfWork.CourseRepository.AddAsync(newCourse);
            var scoreStructure = Generator.GenerateScoreStructureForCourse(newCourseId.ToString());
            await _unitOfWork.ScoreStructureRepository.AddAsync(scoreStructure);
            await _unitOfWork.SaveChangesAsync();
            var dto = mapper.Map<CourseDto>(newCourse);
            dto.ScoreStructure = mapper.Map<ScoreStructureDto>(scoreStructure);
            return new ApiResponse<object>(200, "Thành công", dto);
        }
        public async Task<ApiResponse<object>> UpdateCourseAsync(string courseId,CreateCourseRequestDto requestDto,string userId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy lớp");
            }
            if (course.LecturerId != userId)
            {
                return new ApiBadRequestResponse<object>("Giáo viên mới có quyền chỉnh sửa");
            }
            if (await _unitOfWork.CourseRepository.AnyAsync(c => c.CourseGroup == requestDto.CourseGroup && c.LecturerId == userId && c.SubjectId == requestDto.SubjectId && c.CourseId != course.CourseId))
            {
                return new ApiBadRequestResponse<object>("Nhóm môn học được mở không được trùng");
            }
            if (await _unitOfWork.CourseRepository.AnyAsync(c => c.CourseId != courseId && c.InviteCode == requestDto.InviteCode))
            {
                return new ApiBadRequestResponse<object>("Mã mời không được trùng");
            }
            course.CourseGroup = requestDto.CourseGroup;
            course.EnableInvite = requestDto.EnableInvite;
            course.InviteCode = requestDto.InviteCode;
            course.UpdatedAt = DateTime.Now;
            course.SubjectId = requestDto.SubjectId;
            course.IsHidden = requestDto.IsHidden;
            course.Name = requestDto.Name;
            course.Semester = requestDto.Semester;
            _unitOfWork.CourseRepository.Update(course);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Cập nhật thành công", mapper.Map<CourseDto>(course));
            }
            return new ApiBadRequestResponse<object>("Cập nhật lớp học thất bại");
        }
        public async Task<ApiResponse<object>> GetCourseByIdAsync(string courseId)
        {
            var data = await GetCourseDtoByIdAsync(courseId);
            if(data == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy khóa học");
            }
            return new ApiResponse<object>(200, "Thành công", data);
        }
        public async Task<ApiResponse<object>> ApplyInviteCodeAsync(string inviteCode,string userId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.InviteCode == inviteCode);
            if (course == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy lớp học");
            }
            if (course.InviteCode != inviteCode)
            {
                return new ApiBadRequestResponse<object>("Mã lớp học không chính xác");
            }
            if(course.EnableInvite == false)
            {
                return new ApiBadRequestResponse<object>("Lớp học hiện đang không cho phép tham gia qua lời mời");
            }
            if(userId == course.LecturerId)
            {
                return new ApiBadRequestResponse<object>("Bạn là giáo viên của lớp");
            }
            if (!await _unitOfWork.EnrolledCourseRepository.AnyAsync(c => c.CourseId == course.CourseId && c.StudentId == userId))
            {
                await _unitOfWork.EnrolledCourseRepository.AddAsync(new EnrolledCourse()
                {
                    CourseId = course.CourseId,
                    StudentId = userId,
                    CreatedAt = DateTime.Now,
                });
                var result = await _unitOfWork.SaveChangesAsync();
                if (result < 0)
                {
                    return new ApiBadRequestResponse<object>("Không thể tham gia lớp học");
                }
            }
            else
            {
                return new ApiBadRequestResponse<object>("Bạn đã tham gia lớp học");

            }
             return await GetCourseByIdAsync(course.CourseId);
        }
        public async Task<ApiResponse<object>> GetStudentsWithoutGroupsAsync(string courseId)
        {
            //Get students in course
            var enrolledData = await _unitOfWork.EnrolledCourseRepository.GetAllAsync();
            var usersData = await _userManager.Users.ToListAsync();
            var users = from user in usersData
                        join enroll in enrolledData on user.Id equals enroll.StudentId
                        where enroll.CourseId == courseId
                        select user;
            var enrolledStudents = mapper.Map<List<UserDto>>(users.ToList());
            //Get group in course
            var groups = await _unitOfWork.GroupRepository.FindByCondition(c => c.CourseId.Equals(courseId),false,c => c.GroupMembers).ToListAsync();

            var studentWithoutGroups = new List<UserDto>();
            foreach(var student in enrolledStudents)
            {
                if (!groups.Any(gr=>gr.GroupMembers
                        .Any(g=>g.StudentId.Equals(student.Id))))
                {
                    studentWithoutGroups.Add(student);
                }
            }    
            return new ApiResponse<object>(200, "Thành công",studentWithoutGroups);
        }
        public async Task<ApiResponse<object>> DeleteCourseAsync(string courseId)
        {
            var currentUserId = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var course = await  _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return new ApiNotFoundResponse<object>($"Không thể tìm thấy lớp học với id {courseId}");
            }
            if( currentUserId != course.LecturerId)
            {
                return new ApiBadRequestResponse<object>("Chỉ có giảng viên của lớp mới có quyền xóa");
            }    
            _unitOfWork.CourseRepository.Delete(course);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Xóa thành công", mapper.Map<CourseDto>(course));
            }
            return new ApiBadRequestResponse<object>("Xóa thông tin lớp học thất bại");
        }
        public async Task<ApiResponse<object>> GetProjectsInCourseAsync(string courseId)
        {
            var projects = _unitOfWork.ProjectRepository.GetAll(c=>c.CourseId == courseId);
            var projectDtos = mapper.Map<List<ProjectDto>>(projects.ToList());

            foreach(var projectDto in projectDtos)
            {
                projectDto.CreateUser = mapper.Map<UserDto>(await _userManager.FindByIdAsync(projectDto.CreateUserId));
            }
            return new ApiResponse<object>(200, "Thành công", projectDtos);
        }
        public async Task<ApiResponse<object>> GetGroupsInCourseAsync(string courseId)
        {
            var groups = await _unitOfWork.GroupRepository.GetAllAsync();
            var groupIds = groups.Where(c=>c.CourseId == courseId).Select(c=>c.GroupId).ToList();
            var groupsDto = new List<GroupDto>();
            foreach(var groupId in groupIds)
            {
                groupsDto.Add(await groupService.GetGroupDtoAsync(groupId));
            }
            return new ApiSuccessResponse<object>(200, "Lấy dữ liệu thành công", groupsDto);
        }
        public async Task<ApiResponse<object>> GetRegenerateInviteCodeAsync(string courseId)
        {
            var currentUserId = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy lớp");
            }
            if(currentUserId != course.LecturerId)
            {
                return new ApiBadRequestResponse<object>("Chỉ có giáo viên của lớp mới có quyền này");
            }    
            course.InviteCode = await SuggestInviteCode();
            _unitOfWork.CourseRepository.Update(course);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Tạo mã mời thành công", course.InviteCode);
        }
        public async Task<ApiResponse<object>> GetSuggestInviteCodeAsync()
        {
            var code = await SuggestInviteCode();   
            return new ApiResponse<object>(200, "Tạo mã mời thành công", code);
        }
        public async Task<ApiResponse<object>> GetToggleInviteCodeAsync(string courseId,bool isHidden)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy lớp");
            }
            course.IsHidden = isHidden;
            _unitOfWork.CourseRepository.Update(course);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<object>(200, "Thành công");
        }
        public async Task<ApiResponse<object>> RemoveStudentFromCourseAsync(string courseId, string[] studentIds,string currentUserId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy khóa học");
            }
            if(course.LecturerId != currentUserId)
            {
                return new ApiBadRequestResponse<object>("Chỉ giáo viên mới có quyền xóa học viên");
            }
            foreach (var studentId in studentIds)
            {
                var enrollData = await _unitOfWork.EnrolledCourseRepository.GetFirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == courseId);
                if(enrollData != null)
                    _unitOfWork.EnrolledCourseRepository.Delete(enrollData);
            }
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Xóa thành viên thành công");
        }
        public async Task<ApiResponse<object>> GetFindCourseByInviteCodeAsync(string inviteCode)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.InviteCode == inviteCode);
            if (course == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy khóa học");
            }
            return new ApiResponse<object>(200, "Tìm thấy khóa học", course);
        }
        #endregion

        #region for_service
        public async Task<CourseDto> GetCourseDtoByIdAsync(string courseId, bool isLoadAnnoucements = true, bool isLoadStudent = true,bool isLoadAssignment = true)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return null;
            }
            var courseDto = mapper.Map<CourseDto>(course);
            courseDto.Subject = mapper.Map<SubjectDto>(await _unitOfWork.SubjectRepository.GetFirstOrDefaultAsync(c => c.SubjectId == courseDto.SubjectId));
            courseDto.Lecturer = mapper.Map<UserDto>(await _userManager.FindByIdAsync(courseDto.LecturerId));
            if (isLoadAnnoucements)
            {
                courseDto.Announcements = await annoucementService.GetAnnouncementDtosInCourseAsync(courseId);
            }
            if (isLoadStudent)
            {
                var enrolledData = await _unitOfWork.EnrolledCourseRepository.GetAllAsync();
                var usersData = await _userManager.Users.ToListAsync();
                var users = from user in usersData
                            join enroll in enrolledData on user.Id equals enroll.StudentId
                            where enroll.CourseId == courseId
                            select user;
                courseDto.Students = mapper.Map<List<UserDto>>(users.ToList());
            }
            if (isLoadAssignment)
            {
                var assignments = _unitOfWork.AssignmentRepository.GetAll(c => c.CourseId == courseDto.CourseId);
                courseDto.Assignments = mapper.Map<List<AssignmentNoCourseDto>>(assignments);
            }
            return courseDto;

        }
        public async Task<string> SuggestInviteCode()
        {
            var suggestCode = GenerateRandomNumericString(6);
            while (await _unitOfWork.CourseRepository.AnyAsync(c => c.InviteCode == suggestCode))
            {
                suggestCode = GenerateRandomNumericString(6);
            }
            return suggestCode;
        }
        public async Task<bool> CheckIsTeacherAsync(string userId,string courseId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c=>c.CourseId == courseId);   
            if(course == null)
            {
                return false;
            }
            if(course.LecturerId != userId)
            {
                return false;
            }
            return true;
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
        
        #endregion



    }
}
