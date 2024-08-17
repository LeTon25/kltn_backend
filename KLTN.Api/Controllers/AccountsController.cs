using AutoMapper;
using KLTN.Api.Services.Interfaces;
using KLTN.Application.DTOs.Accounts;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Response;
using KLTN.Domain;
using KLTN.Domain.Entities;
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
        public AccountsController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            IMapper mapper) 
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
            this._tokenService = tokenService;  
            this._mapper = mapper;
        }
        [HttpPost("register")]
        [ApiValidationFilter]
        public async Task<IActionResult> Register(RegisterRequestDto requestDto)
        {
            var users = _userManager.Users;
            if (await users.AnyAsync(c => c.UserName == requestDto.UserName))
            {
                return BadRequest(new ApiBadRequestResponse("Tên người dùng không được trùng"));
            }
            if (await users.AnyAsync(c => c.Email == requestDto.Email))
            {
                return BadRequest(new ApiBadRequestResponse("Email người dùng không được trùng"));
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
                var authResponse = new AuthResponseDto
                {
                    Token = _tokenService.GenerateTokens(user),
                    RefreshToken = _tokenService.GenerateRefreshToken(),
                    ExpireIn = DateTime.Now.AddHours(12),
                    User = _mapper.Map<UserDto>(user),
                };
                user.RefreshToken = authResponse.RefreshToken;
                user.RefreshTokenExpiry = authResponse.ExpireIn;

                await _userManager.UpdateAsync(user);
                return Ok(authResponse);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse(result));
            }
        }

        [HttpPost("login")]
        [ApiValidationFilter]
        public async Task<IActionResult> Login(LoginRequestDto requestDto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x=>x.UserName == requestDto.UserName);

            if (user == null) 
            { 
                return Unauthorized("Không tìm thấy tên người dùng");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user,requestDto.Password,false);

            if (!result.Succeeded)
            {
                return Unauthorized("Username hoặc mật khẩu bị sai");
            }
            var authResponse  = new AuthResponseDto
            {
                Token = _tokenService.GenerateTokens(user),
                RefreshToken = _tokenService.GenerateRefreshToken(),
                ExpireIn = DateTime.Now.AddHours(12),
                User = _mapper.Map<UserDto>(user), 
            };
            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpiry = authResponse.ExpireIn;

            await _userManager.UpdateAsync(user);
            return Ok(authResponse);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenModel model)
        {
            var principal = _tokenService.GetTokenPrincipal(model.Token);
            if(principal?.Identity?.Name is null)
            {
                return BadRequest("Không thể cấp mới được token");
            }
            var user = await _userManager.FindByNameAsync(principal.Identity.Name);
            if (user is null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiry < DateTime.Now)
                return BadRequest("Không thể cấp mới được token");

            var refreshToken = _tokenService.GenerateRefreshToken();
            var expireRefreshToken = DateTime.Now.AddHours(12);
            var authResponse =new AuthResponseDto
            {
                Token = _tokenService.GenerateTokens(user),
                RefreshToken = refreshToken,
                ExpireIn = expireRefreshToken,
            };

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = expireRefreshToken;   

            await _userManager.UpdateAsync(user);
            return Ok(authResponse);
        }

    }
}
