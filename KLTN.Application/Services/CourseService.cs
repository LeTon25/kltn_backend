using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using KLTN.Domain.Entities;
using AutoMapper;
using KLTN.Application.Helpers.Response;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Users;
using KLTN.Application.DTOs.Subjects;
using KLTN.Application.DTOs.Projects;
using Microsoft.EntityFrameworkCore;
using KLTN.Application.DTOs.Groups;
using KLTN.Application.DTOs.Assignments;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using KLTN.Domain.Util;
using KLTN.Application.DTOs.ScoreStructures;
using System.Text.RegularExpressions;
using KLTN.Application.DTOs.Settings;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Metadata;
using KLTN.Domain;
using KLTN.Application.DTOs.Comments;
using KLTN.Domain.Enums;
using KLTN.Application.DTOs.Submissions;
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
        private readonly ScoreStructureService scoreStructureService;
        public CourseService(IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            IMapper mapper,
            AnnoucementService annoucementService,
            GroupService groupService,
            ScoreStructureService scoreStructureService,
            IHttpContextAccessor httpContextAccessor) 
        { 
            this._unitOfWork = unitOfWork;
            this._userManager = userManager;
            this.mapper = mapper;  
            this.annoucementService = annoucementService;
            this.groupService = groupService;
            this.scoreStructureService = scoreStructureService;
            this.httpContextAccessor = httpContextAccessor;
        }
        #region for controller
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
                EnableInvite = true,
                InviteCode = requestDto.InviteCode ?? GenerateRandomNumericString(6),
                LecturerId = currentUserId!,
                SubjectId = requestDto.SubjectId,
                CreatedAt = DateTime.Now,
                Semester = requestDto.Semester,
                UpdatedAt = null,
                DeletedAt = null,
                Name = requestDto.Name,
            };
            await _unitOfWork.CourseRepository.AddAsync(newCourse);
            // Tự động tạo điểm khi tạo lớp học
            var scoreStructure = Generator.GenerateScoreStructureForCourse(newCourseId.ToString());
            await _unitOfWork.ScoreStructureRepository.AddAsync(scoreStructure);
            // Tự động tạo setting khi tạo lớp học
            var newSetting = new Setting()
            {
                SettingId = Guid.NewGuid().ToString(),
                CourseId = newCourseId.ToString(),
                AllowStudentCreateProject = false,
                HasFinalScore = false,
                MaxGroupSize = null,
                MinGroupSize = null
            };
            await _unitOfWork.SettingRepository.AddAsync(newSetting);
            //Tạo assign cho end term
            var endtermScore = scoreStructure.Children!.FirstOrDefault(c=>c.ColumnName.Equals(Constants.Score.EndtermColumnName));
            var endtermAssignment = new Assignment()
            {
                AssignmentId = Guid.NewGuid().ToString(),
                CourseId = newCourseId.ToString(),
                ScoreStructureId = endtermScore!.Id,
                Title = "Bài nộp cuối kỳ",
                Type = Constants.AssignmentType.Final,
                Content = "Nơi để học sinh nộp bài cuối kỳ",
                Attachments = new List<Domain.Entities.File>(),
                AttachedLinks = new List<MetaLinkData>(),
                IsGroupAssigned = true,
                CreatedAt = DateTime.Now,
            };
            await _unitOfWork.AssignmentRepository.AddAsync(endtermAssignment);
            await _unitOfWork.SaveChangesAsync();

            var dto = mapper.Map<CourseDto>(newCourse);
            dto.ScoreStructure = mapper.Map<ScoreStructureDto>(scoreStructure);
            dto.Setting = mapper.Map<SettingDto>(newSetting);
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
            var groups = await _unitOfWork.GroupRepository.FindByCondition(c=>c.CourseId.Equals(courseId) && c.GroupType.Equals(Constants.GroupType.Final),false).ToListAsync();
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
        public async Task<ApiResponse<object>> RemoveStudentFromCourseAsync(string courseId, List<string> studentIds,string currentUserId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == courseId,false,c=>c.EnrolledCourses);
            if (course == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy khóa học");
            }
            if(course.LecturerId != currentUserId)
            {
                return new ApiBadRequestResponse<object>("Chỉ giáo viên mới có quyền xóa học viên");
            }
            if(course.EnrolledCourses == null || course.EnrolledCourses.Count == 0)
            {
                return new ApiResponse<object>(200,"Lớp không có học viên",null);
            }
            foreach (var studentId in studentIds)
            {
                var enrollData = course.EnrolledCourses.Where(c=>c.CourseId.Equals(courseId) && c.StudentId.Equals(studentId)).FirstOrDefault();
                if(enrollData != null)
                    _unitOfWork.EnrolledCourseRepository.Delete(enrollData);
            }
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Xóa thành viên thành công");
        }
        public async Task<ApiResponse<object>> AddStudentToCourseAsync(string courseId, AddStudentRequestDto dto, string currentUserId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId.Equals(courseId),false, c=>c.EnrolledCourses);
            if (course == null) 
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy lớp học");
            }
            if(course.LecturerId != currentUserId)
            {
                return new ApiBadRequestResponse<object>("Chỉ có giáo viên mới có quyền này");
            }
            var enrollDatas = course.EnrolledCourses != null ? course.EnrolledCourses : new List<EnrolledCourse>();
            if(dto.Emails != null && dto.Emails.Count > 0)
            {
                foreach(var email in dto.Emails)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user != null)
                    { 
                        if(enrollDatas.Any(c=>c.StudentId.Equals(user.Id)))
                        {
                            return new ApiBadRequestResponse<object>("Có người dùng đã tham gia lớp học");
                        }
                        var newEnrollData = new EnrolledCourse()
                        {
                            StudentId = user.Id,
                            CourseId = courseId,
                            CreatedAt = DateTime.UtcNow,
                        };
                        await _unitOfWork.EnrolledCourseRepository.AddAsync(newEnrollData);
                        enrollDatas.Add(newEnrollData);
                    }
                }    
            }
            await _unitOfWork.SaveChangesAsync();
            var responseDto = await GetCourseDtoByIdAsync(courseId);
            return new ApiResponse<object>(200,"Thêm thành công",responseDto);
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
        public async Task<ApiResponse<StatisticDto>> GetStatisticAsync(string courseId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId.Equals(courseId), false, c => c.EnrolledCourses,c => c.Projects);
            var groups = await _unitOfWork.GroupRepository.FindByCondition(c => c.CourseId.Equals(course.CourseId) && c.GroupType.Equals(Constants.GroupType.Final), false, c => c.GroupMembers).ToListAsync();

            var allStudentIdsInCourseCount = 0;
            var allStudentIdsHasGroupCount = 0;

            if(course.EnrolledCourses != null)
                allStudentIdsInCourseCount = course.EnrolledCourses.Select(c=>c.StudentId).ToList().Count;
            if(groups != null)
            {
                foreach(var group in groups)
                {
                    if (group.GroupMembers != null)
                        allStudentIdsHasGroupCount += group.GroupMembers.Count; 
                }    
            }
            var response = new ApiResponse<StatisticDto>(200,"", new StatisticDto
            {
                NumberOfGroups = groups != null ? groups.Count : 0,
                NumberOfProjects = course.Projects != null ? course.Projects.Count : 0,
                NumberOfUngroupStudents = allStudentIdsInCourseCount - allStudentIdsHasGroupCount
            });
            return response ;
           
        }
        public async Task<ApiResponse<AssignmentDto>> GetEndTermAsync(string courseId,string currentUserId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId.Equals(courseId), false, c => c.Lecturer!,c => c.Setting!);
            if(!course.Setting!.HasFinalScore)
            {
                return new ApiResponse<AssignmentDto>(400, "Lớp học không có đồ án cuối kì");
            }
            var assignment = await _unitOfWork.AssignmentRepository.GetFirstOrDefaultAsync(c=>c.CourseId.Equals(courseId) && c.Type==Constants.AssignmentType.Final,false,c=>c.ScoreStructure!);
            
            var assignmentDto = mapper.Map<AssignmentDto>(assignment);
            assignmentDto.CreateUser = mapper.Map<UserDto>(course.Lecturer);

            var comments = await _unitOfWork.CommentRepository.FindByCondition(c => c.CommentableId.Equals(assignment.AssignmentId) && c.CommentableType.Equals(CommentableType.Assignment), false, c => c.User!).ToListAsync();
            assignmentDto.Comments = mapper.Map<List<CommentDto>>(comments);
            
            if (currentUserId != course.LecturerId)
            {
                var submission = await _unitOfWork.SubmissionRepository.GetFirstOrDefaultAsync(c => c.AssignmentId.Equals(assignment.AssignmentId) && c.UserId.Equals(currentUserId), false, c => c.CreateUser!);
                assignmentDto.Submission = mapper.Map<SubmissionDto>(submission);
            }
            return new ApiResponse<AssignmentDto>(200, "Thành công", assignmentDto);

        }
        public async Task<ApiResponse<ArchiveCourseResult>> ArchiveCourseAsync(string courseId, string currentUserId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == courseId);
            if(course == null)
            {
                return new ApiNotFoundResponse<ArchiveCourseResult>("Không tìm thấy lớp học",new ArchiveCourseResult
                {
                    Result = false
                });
            }
            if(course.LecturerId != currentUserId)
            {
                return new ApiResponse<ArchiveCourseResult>(403,"Chỉ có giáo viên mới có thể lữu trữ lớp học", new ArchiveCourseResult
                {
                    Result = false
                });
            }
            course.SaveAt = DateTime.Now;
            _unitOfWork.CourseRepository.Update(course);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<ArchiveCourseResult>(200, "Thành công", new ArchiveCourseResult
            {
                Result = true
            });

        }
        public async Task<ApiResponse<ArchiveCourseResult>> CancelArchiveCourseAsync(string courseId, string currentUserId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return new ApiNotFoundResponse<ArchiveCourseResult>("Không tìm thấy lớp học", new ArchiveCourseResult
                {
                    Result = false
                });
            }
            if (course.LecturerId != currentUserId)
            {
                return new ApiResponse<ArchiveCourseResult>(403, "Chỉ có giáo viên mới có thể lữu trữ lớp học", new ArchiveCourseResult
                {
                    Result = false
                });
            }
            course.SaveAt = null;
            _unitOfWork.CourseRepository.Update(course);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<ArchiveCourseResult>(200, "Thành công", new ArchiveCourseResult
            {
                Result = true
            });

        }
        #endregion

        #region for_service
        public async Task<CourseDto> GetCourseDtoByIdAsync(string courseId, bool isLoadAnnoucements = true, bool isLoadStudent = true,bool isLoadAssignment = true,bool isLoadScore=true)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId == courseId,false,c=>c.Setting!, c => c.Lecturer!,c =>c.Subject!,c => c.EnrolledCourses);
            if (course == null)
            {
                return null;
            }
            var courseDto = mapper.Map<CourseDto>(course);
            if (isLoadAnnoucements)
            {
                courseDto.Announcements = await annoucementService.GetAnnouncementDtosInCourseAsync(courseId);
            }
            if (isLoadStudent)
            {
                var studentIds = course.EnrolledCourses.Select(c=>c.StudentId).ToList();
                var students = new List<User>();
                if(studentIds != null && studentIds.Count > 0)
                {
                    students = await _unitOfWork.UserRepository.FindByCondition(c => studentIds.Contains(c.Id)).ToListAsync();
                }    
                courseDto.Students = mapper.Map<List<UserDto>>(students);
            }
            if (isLoadScore)
            {
                var score = await _unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c => c.CourseId.Equals(courseId) && c.ParentId == null, false);
                if (score != null)
                {
                    await scoreStructureService.LoadChildrenAsync(score);
                    courseDto.ScoreStructure = mapper.Map<ScoreStructureDto>(score);
                }
            }
            if (isLoadAssignment)
            {
                var assignments = await _unitOfWork.AssignmentRepository.FindByCondition(c=>c.CourseId.Equals(courseId) && c.Type != Constants.AssignmentType.Final,false, c=>c.ScoreStructure!).ToListAsync();
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
