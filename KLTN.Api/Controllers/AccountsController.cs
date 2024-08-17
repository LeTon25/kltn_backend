using AutoMapper;
using KLTN.Api.Services.Interfaces;
using KLTN.Application.DTOs.Accounts;
using KLTN.Application.DTOs.Announcements;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Domain;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : Controller
    {
        private readonly UserManager<User> _userManager;    
        private readonly SignInManager<User> _signInManager;    
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _db;
        public AccountsController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            IMapper mapper,
            ApplicationDbContext _db) 
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
            this._tokenService = tokenService;  
            this._mapper = mapper;
            this._db = _db;
        }
        [HttpPost("register")]
        [ApiValidationFilter]
        public async Task<IActionResult> Register(RegisterRequestDto requestDto)
        {
            var users = _userManager.Users;
            if (await users.AnyAsync(c => c.UserName == requestDto.UserName))
            {
                return BadRequest(new ApiBadRequestResponse<string>("Tên người dùng không được trùng"));
            }
            if (await users.AnyAsync(c => c.Email == requestDto.Email))
            {
                return BadRequest(new ApiBadRequestResponse<string>("Email người dùng không được trùng"));
            }
            var result = await _userManager.CreateAsync(new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = requestDto.UserName,
                Email = requestDto.Email,
                LockoutEnabled = false,
                Gender = true,
                DoB = null,
                UserType = Domain.Enums.UserType.Student,
                FullName = "",
                CreatedAt = DateTime.Now,
            },requestDto.Password);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(requestDto.UserName);
                await _userManager.AddToRoleAsync(user, Constants.Role.Student);

                DateTime expiresAt = DateTime.Now.AddMinutes(30);
                var authResponse = new AuthResponseDto
                {
                    Token = await _tokenService.GenerateTokens(user,expiresAt),
                    RefreshToken = _tokenService.GenerateRefreshToken(),
                    ExpiresAt = expiresAt,
                    User = _mapper.Map<UserDto>(user),
                };
                user.RefreshToken = authResponse.RefreshToken;
                user.RefreshTokenExpiry = authResponse.ExpiresAt;

                await _userManager.UpdateAsync(user);
                return Ok(new ApiResponse<AuthResponseDto>(200,"Đăng kí thành công",authResponse));
            }
            else
            {
                return Ok(new ApiBadRequestResponse<string>(result));
            }
        }

        [HttpPost("login")]
        [ApiValidationFilter]
        public async Task<IActionResult> Login(LoginRequestDto requestDto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x=>x.UserName == requestDto.UserName);

            if (user == null) 
            { 
                return Ok(new ApiResponse<string>(401,"Tên người dùng không đúng"));
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user,requestDto.Password,false);

            if (!result.Succeeded)
            {
                return Unauthorized(new ApiResponse<string>(401, "Thông tin đăng nhập không chính xác"));
            }
            DateTime expiresAt = DateTime.Now.AddMinutes(30);
            var authResponse  = new AuthResponseDto
            {
                Token = await _tokenService.GenerateTokens(user,expiresAt),
                RefreshToken = _tokenService.GenerateRefreshToken(),
                ExpiresAt = expiresAt, //access_token
                User = _mapper.Map<UserDto>(user), 
            };
            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpiry = DateTime.Now.AddHours(12);

            await _userManager.UpdateAsync(user);
            return Ok(new ApiSuccessResponse<AuthResponseDto>(200,"Đăng nhập thành công",authResponse));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenModel model)
        {
            var principal = _tokenService.GetTokenPrincipal(model.Token);
            if(principal?.Identity?.Name is null)
            {
                return BadRequest(new ApiBadRequestResponse<string>("Không thể cấp mới token"));
            }
            var user = await _userManager.FindByNameAsync(principal.Identity.Name);
            if (user is null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiry < DateTime.Now)
                return BadRequest(new ApiBadRequestResponse<string>("Không thể cấp mới token"));
            DateTime expiresAt = DateTime.Now.AddMinutes(30);
            var response = new RefreshTokenResponseDto
            {
                Token = await _tokenService.GenerateTokens(user, expiresAt),
                ExpiresAt = expiresAt, //access_token
            };
            return Ok(new ApiSuccessResponse<RefreshTokenResponseDto>(200,"Refresh token thành công",response));
        }

        [HttpGet("courses")]
        [Authorize]
        public async Task<IActionResult> GetAllCoursesByCurrentUser()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            #region lay cac khoa nguoi dung giang day
            var teachingCourse = from user in _db.Users where user.Id == userId
                                 join course in _db.Courses on user.Id equals course.LecturerId
                                 join subject in _db.Subjects on course.SemesterId equals subject.SubjectId into courseSubject
                                 from subject in courseSubject.DefaultIfEmpty()
                                 join semester in _db.Semesters on course.SemesterId equals semester.SemesterId into courseSemester
                                 from semester in courseSemester.DefaultIfEmpty()
                                 select new CourseDto
                                 {
                                     CourseId = course.CourseId,
                                     SubjectId = course.SubjectId,
                                     SemesterId = course.SemesterId,
                                     CourseGroup = course.CourseGroup,
                                     Background = course.Background,
                                     InviteCode = course.InviteCode,
                                     EnableInvite = course.EnableInvite,
                                     LecturerId = course.LecturerId,
                                     CreatedAt = course.CreatedAt,
                                     UpdatedAt = course.UpdatedAt,
                                     DeletedAt = course.DeletedAt,
                                     SubjectName = subject != null ? subject.Name : "Không tìm thấy",
                                     SemesterName = semester != null ? semester.Name : "Không tìm thấy",
                                     LecturerName = user != null ? user.FullName : "Không tìm thấy"
                                 };
            #endregion
            #region lay cac khoa nguoi dung tham gia hoc
            var enrolledCourses = from user in _db.Users where user.Id == userId
                                 join enrolledStudent in _db.EnrolledCourse on user.Id equals enrolledStudent.StudentId
                                 join course in _db.Courses on enrolledStudent.CourseId equals course.CourseId
                                 join subject in _db.Subjects on course.SemesterId equals subject.SubjectId into courseSubject
                                 from subject in courseSubject.DefaultIfEmpty()
                                 join semester in _db.Semesters on course.SemesterId equals semester.SemesterId into courseSemester
                                 from semester in courseSemester.DefaultIfEmpty()
                                 select new CourseDto
                                 {
                                     CourseId = course.CourseId,
                                     SubjectId = course.SubjectId,
                                     SemesterId = course.SemesterId,
                                     CourseGroup = course.CourseGroup,
                                     Background = course.Background,
                                     InviteCode = course.InviteCode,
                                     EnableInvite = course.EnableInvite,
                                     LecturerId = course.LecturerId,
                                     CreatedAt = course.CreatedAt,
                                     UpdatedAt = course.UpdatedAt,
                                     DeletedAt = course.DeletedAt,
                                     SubjectName = subject != null ? subject.Name : "Không tìm thấy",
                                     SemesterName = semester != null ? semester.Name : "Không tìm thấy",
                                     LecturerName = user != null ? user.FullName : "Không tìm thấy"
                                 };
            #endregion

            return Ok(new ApiResponse<CourseByUserDto>(200,"Thành công",new CourseByUserDto()
            {
                CreatedCourses = await teachingCourse.ToListAsync(),
                EnrolledCourses = await enrolledCourses.ToListAsync(),
            }));
        }

        [HttpGet("announcements/filter")]
        [Authorize]
        public async Task<IActionResult> GetAllAnnouncementsByCurrentUser(string filter,int pageIndex, int pageSize)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = from announcement in _db.Announcements where announcement.UserId == userId
                        join user in _db.Users on announcement.UserId equals user.Id into announcementUsers
                        from user in announcementUsers.DefaultIfEmpty()
                        select new AnnouncementDto
                        {
                            AnnouncementId = announcement.AnnouncementId,
                            UserId = announcement.UserId,
                            CourseId = announcement.CourseId,
                            Content = announcement.Content,
                            AttachedLinks = announcement.AttachedLinks,
                            CreatedAt = announcement.CreatedAt,
                            UpdatedAt = announcement.UpdatedAt,
                            DeletedAt = announcement.DeletedAt,
                            CreateUserName = user.FullName
                        };
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            var data = items.Select(e => _mapper.Map<AnnouncementDto>(e)).ToList();

            var pagination = new Pagination<AnnouncementDto>
            {
                Items = data,
                TotalRecords = totalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(new ApiResponse<Pagination<AnnouncementDto>>(200, "Thành công", pagination));
        }

    }
}
