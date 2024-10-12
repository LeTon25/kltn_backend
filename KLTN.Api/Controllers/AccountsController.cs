using AutoMapper;
using KLTN.Api.Services.Interfaces;
using KLTN.Application.DTOs.Accounts;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Response;
using KLTN.Application.Services;
using KLTN.Domain;
using KLTN.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly AccountService _accountService;
        //private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        public AccountsController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            IMapper mapper,
            AccountService accountService,
            //IEmailSender emailSender,
            IConfiguration configuration
          ) 
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
            this._tokenService = tokenService;  
            this._mapper = mapper;
            this._accountService = accountService;
            //this._emailSender = emailSender;
            this._configuration  = configuration;
        }
        [HttpPost("register")]
        [ApiValidationFilter]
        public async Task<IActionResult> Register(RegisterRequestDto requestDto)
        {
            var users = _userManager.Users;
            if (await users.AnyAsync(c => c.UserName == requestDto.UserName || c.CustomId == requestDto.UserName))
            {
                return BadRequest(new ApiBadRequestResponse<string>("Tên người dùng không được trùng"));
            }
            if (await users.AnyAsync(c => c.Email == requestDto.Email))
            {
                return BadRequest(new ApiBadRequestResponse<string>("Email người dùng không được trùng"));
            }
            if (await users.AnyAsync(c => c.CustomId == requestDto.CustomId || c.UserName == requestDto.CustomId))
            {
                return BadRequest(new ApiBadRequestResponse<string>("Mã cán bộ/sinh viên không được trùng"));
            }
            var result = await _userManager.CreateAsync(new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = requestDto.UserName,
                Email = requestDto.Email,
                LockoutEnabled = false,
                Gender = "Nam",
                DoB = null,
                CustomId = requestDto.CustomId,
                UserType = Domain.Enums.UserType.Student,
                FullName = requestDto.FullName,
                CreatedAt = DateTime.Now,
            },requestDto.Password);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(requestDto.UserName);
                await _userManager.AddToRoleAsync(user, Constants.Role.Student);

                DateTime expiresAt = DateTime.Now.AddDays(2);
                DateTime refreshTokenExpiresAt = DateTime.Now.AddMonths(2);
                string token = await _tokenService.GenerateTokens(user, expiresAt);
                var authResponse = new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = _tokenService.GenerateRefreshToken(),
                    TokenExpiresAt = expiresAt,
                    RefreshTokenExpiresAt = refreshTokenExpiresAt,
                    User = _mapper.Map<UserDto>(user),
                };
                user.RefreshToken = authResponse.RefreshToken;
                user.RefreshTokenExpiry = refreshTokenExpiresAt;

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
            var user = await _userManager.Users.FirstOrDefaultAsync(
               x => 
               x.UserName == requestDto.Identifier 
            || x.Email == requestDto.Identifier
            || x.CustomId == requestDto.Identifier);


            if (user == null) 
            { 
                return Ok(new ApiResponse<string>(401,"Thông tin đăng nhập không đúng"));
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user,requestDto.Password,false);

            if (!result.Succeeded)
            {
                return Unauthorized(new ApiResponse<string>(401, "Thông tin đăng nhập không chính xác"));
            }
            DateTime expiresAt = DateTime.Now.AddDays(2);
            DateTime refreshTokenExpiresAt = DateTime.Now.AddMonths(2);
            string token = await _tokenService.GenerateTokens(user, expiresAt);

            var authResponse = new AuthResponseDto
            {
                Token = token,
                RefreshToken = _tokenService.GenerateRefreshToken(),
                TokenExpiresAt = expiresAt,
                RefreshTokenExpiresAt = refreshTokenExpiresAt,
                User = _mapper.Map<UserDto>(user),
            };
            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpiry = refreshTokenExpiresAt;

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
            DateTime expiresAt = DateTime.Now.AddDays(2);
            string token = await _tokenService.GenerateTokens(user, expiresAt);
            var response = new RefreshTokenResponseDto
            {
                Token = token,
                TokenExpiresAt = expiresAt, 
            };
            return Ok(new ApiSuccessResponse<RefreshTokenResponseDto>(200,"Refresh token thành công",response));
        }

        //public async Task<IActionResult> GeneratePasswordResetTokenAsync(GeneratePasswordRequestDto requestDto)

        [HttpGet("courses")]
        [Authorize]
        public async Task<IActionResult> GetAllCoursesByCurrentUserAsync()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse( await _accountService.GetCoursesByCurrentUserAsync(userId));
        }
        //[HttpPost("forgot-password")]
        //public async Task<IActionResult> ForgotPassword(ForgetPasswordRequestDto forgotPasswordDto)
        //{
        //    var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        //    if (user == null)
        //        return BadRequest(new ApiBadRequestResponse<string>("Email không hợp lệ"));

        //    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        //    var callbackUrl = $"{_configuration.GetSection("ClientUrl").Value}/reset-password?token={resetToken}&email={user.Email}";

        //    await _emailSender.SendEmailAsync(forgotPasswordDto.Email, "Đặt lại mật khẩu",
        //        $"Vui lòng click vào link sau đây để đặt lại mật khẩu : <a href='{callbackUrl}'>here</a>.");

        //    return Ok(new ApiResponse<string>(200,"Vui lòng kiểm tra email"));
        //}

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                return BadRequest(new ApiBadRequestResponse<string>("Email không hợp lệ"));

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);

            if (!result.Succeeded)
                return BadRequest(new ApiBadRequestResponse<string>(result));

            return Ok(new ApiResponse<string>(200,"Đổi mật khẩu thành công"));
        }

        protected IActionResult SetResponse(ApiResponse<object> api)
        {
            switch (api.StatusCode)
            {
                case 200:
                    return Ok(api);
                case 400:
                    return BadRequest(api);
                case 401:
                    return Unauthorized(api);
                case 404:
                    return NotFound(api);
                default:
                    return Ok(api);
            }
        }

    }
}
