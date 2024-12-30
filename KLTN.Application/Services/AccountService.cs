using AutoMapper;
using KLTN.Application.DTOs.Accounts;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Requests;
using KLTN.Application.DTOs.Subjects;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Domain;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using KLTN.Domain.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Application.Services
{
    public class AccountService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;
        private readonly ITokenService _tokenService;
        public AccountService(IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            IMapper mapper,
            ITokenService tokenService)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.mapper = mapper;
            this._tokenService = tokenService;
        }
        public async Task<ApiResponse<List<RequestDto>>> GetRequestsByUserAsync(string userId)
        {
            var requests = await unitOfWork.RequestRepository.FindByCondition(c => c.UserId.Equals(userId), false, c => c.Group!, c => c.User!).ToListAsync();
            var courseIds = new List<string>(); 
            foreach(var item in requests)
            {
                item.Group!.Requests = null;
                if (item.Group.GroupType.Equals(Constants.GroupType.Final))
                {
                    courseIds.Add(item.Group.CourseId);
                }
            }
            var finalAssignments = await unitOfWork.AssignmentRepository.FindByCondition(c=>courseIds.Contains(c.CourseId) && c.Type.Equals(Constants.AssignmentType.Final)).ToListAsync();
            foreach(var item in requests)
            {
                if (item.Group!.GroupType.Equals(Constants.GroupType.Final))
                {
                    item.Group.AssignmentId = finalAssignments.FirstOrDefault(c=>c.CourseId.Equals(item.Group.CourseId))!.AssignmentId;
                }
            }
            var dto = mapper.Map<List<RequestDto>>(requests);

            return new ApiResponse<List<RequestDto>>(200, "Lấy dữ liệu thành công",dto);
        }
        public async Task<ApiResponse<object>> GetCoursesByCurrentUserAsync(string userId)
        {
            var subjectData = await unitOfWork.SubjectRepository.GetAllAsync();
            //Khóa học do người dùng giảng dạy
            var teachingCourses =await  unitOfWork.CourseRepository.FindByCondition(c => c.LecturerId == userId && c.SaveAt == null,false,c=>c.Lecturer!,c => c.Setting!,c => c.EnrolledCourses).ToListAsync();
            var teachingCourseDtos = mapper.Map<List<CourseDto>>(teachingCourses);
            
            var lecturer = await userManager.FindByIdAsync(userId);

            foreach (var teachingCourseDto in teachingCourseDtos)
            {
                teachingCourseDto.Subject = mapper.Map<SubjectDto>(subjectData.FirstOrDefault(c=>c.SubjectId==teachingCourseDto.SubjectId));
                teachingCourseDto.Lecturer = mapper.Map<UserDto>(lecturer);
                var data = teachingCourses.Where(c => c.CourseId.Equals(teachingCourseDto.CourseId)).FirstOrDefault();
                teachingCourseDto.StudentCount = data.EnrolledCourses != null ? data.EnrolledCourses.Count : 0;
            }
            //Khóa học do người dùng đăng kí làm học viên
            var enrollData = unitOfWork.EnrolledCourseRepository.GetAll(c => c.StudentId == userId);

            var enrollCourseIds = enrollData.Select(e => e.CourseId).ToList();
            var enrollCourses = await unitOfWork.CourseRepository.FindByCondition(c => enrollCourseIds.Contains(c.CourseId) && c.SaveAt == null,false,c=>c.Setting!,c =>c.Lecturer!,c => c.EnrolledCourses).ToListAsync();

            var enrollCourseDto = mapper.Map<List<CourseDto>>(enrollCourses);
            var lecturerIds = enrollCourseDto.Select(c => c.LecturerId).ToList();

            var lecturers = await unitOfWork.UserRepository.FindByCondition(c => lecturerIds.Contains(c.Id)).ToListAsync();
            foreach (var courseDto in enrollCourseDto)
            {
                courseDto.Subject = mapper.Map<SubjectDto>(subjectData.FirstOrDefault(c => c.SubjectId == courseDto.SubjectId));
                courseDto.Lecturer = mapper.Map<UserDto>(lecturers.FirstOrDefault(c => c.Id == courseDto.SubjectId));
                var data = enrollCourses.Where(c => c.CourseId.Equals(courseDto.CourseId)).FirstOrDefault();
                courseDto.StudentCount = data.EnrolledCourses != null ? data.EnrolledCourses.Count : 0;
            }
            return new ApiResponse<object>(200, "Thành công", new CourseByUserDto()
            {
                CreatedCourses = teachingCourseDtos,
                EnrolledCourses = enrollCourseDto,
            });
        }
        public async Task<ApiResponse<object>> GetArchivedCoursesByCurrentUserAsync(string userId)
        {
            var subjectData = await unitOfWork.SubjectRepository.GetAllAsync();

            var data = new List<CourseDto>();
            // Khóa học do người dùng làm giảng viên
            var teachingCourses = await unitOfWork.CourseRepository.FindByCondition(c => c.LecturerId == userId && c.SaveAt != null).ToListAsync();
            var teachingCourseDtos = mapper.Map<List<CourseDto>>(teachingCourses);
            var lecturer = await userManager.FindByIdAsync(userId);
            foreach (var teachingCourseDto in teachingCourseDtos)
            {
                teachingCourseDto.Subject = mapper.Map<SubjectDto>(subjectData.FirstOrDefault(c => c.SubjectId == teachingCourseDto.SubjectId));
                teachingCourseDto.Lecturer = mapper.Map<UserDto>(lecturer);
            }

            data.AddRange(teachingCourseDtos);
            // Khóa học do người dùng làm học viên
            var enrollData = await unitOfWork.EnrolledCourseRepository.FindByCondition(c => c.StudentId == userId).ToListAsync();

            var enrollCourseIds = enrollData.Select(e => e.CourseId).ToList();
            var enrollCourses = await unitOfWork.CourseRepository.FindByCondition(c => enrollCourseIds.Contains(c.CourseId) && c.SaveAt != null).ToListAsync();

            var enrollCourseDto = mapper.Map<List<CourseDto>>(enrollCourses);

            var lecturerIds = enrollCourseDto.Select(c=>c.LecturerId).ToList();

            var lecturers = await unitOfWork.UserRepository.FindByCondition(c => lecturerIds.Contains(c.Id)).ToListAsync();
            foreach (var courseDto in enrollCourseDto)
            {
                courseDto.Subject = mapper.Map<SubjectDto>(subjectData.FirstOrDefault(c => c.SubjectId == courseDto.SubjectId));
                courseDto.Lecturer = mapper.Map<UserDto>(lecturers.FirstOrDefault(c => c.Id == courseDto.SubjectId));
            }
            data.AddRange(enrollCourseDto);
            return new ApiResponse<object>(200, "Thành công", new ArchivedCourseByUserDto()
            {
               ArchivedCourses = data 
            });
        }
        public async Task<ApiResponse<AuthResponseDto>> HandleLoginByGoogleAsync(string email,string name,string avatar)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = await CreateNewUserLoginGoogleAsync(email, name,avatar);
            }

            var data = await GenerateAuthResponseAsync(user);


            return new ApiResponse<AuthResponseDto>(200,"Đăng nhập thành công", data);

        }
        private async Task<User> CreateNewUserLoginGoogleAsync(string email,string name,string avatar)
        {
            var newUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = email,
                Email = email,
                LockoutEnabled = false,
                Gender = "Nam",
                DoB = null,
                CustomId = "",
                UserType = Domain.Enums.UserType.Student,
                FullName = name,
                Avatar = avatar,
                CreatedAt = DateTime.Now,
            };
            var createResult = await userManager.CreateAsync(newUser);
            if (!createResult.Succeeded)
            {
                throw new Exception("Can not create User");
            }
           var user = await userManager.FindByNameAsync(newUser.UserName);
           await userManager.AddToRoleAsync(user, Constants.Role.User);

           return newUser;
        }
        private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user)
        {
            DateTime expiresAt = DateTime.Now.AddDays(2);
            DateTime refreshTokenExpiresAt = DateTime.Now.AddMonths(2);
            string token = await _tokenService.GenerateTokens(user, expiresAt);
            var role = await userManager.GetRolesAsync(user);

            var authResponse = new AuthResponseDto
            {
                Token = token,
                RefreshToken = _tokenService.GenerateRefreshToken(),
                TokenExpiresAt = expiresAt,
                RefreshTokenExpiresAt = refreshTokenExpiresAt,
                User = mapper.Map<UserDto>(user),
                Role = role.FirstOrDefault()!
            };
            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpiry = refreshTokenExpiresAt;

            await userManager.UpdateAsync(user);
            return authResponse;
        }
    }
}
