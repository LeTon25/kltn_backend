using AutoMapper;
using KLTN.Api.Services.Interfaces;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;
        private readonly IUnitOfWork unitOfWork;
        public UsersController(
           UserManager<User> userManager,
           RoleManager<IdentityRole> roleManager,
           IMapper mapper,
           IStorageService storageService,
           IUnitOfWork unitOfWork
           )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _storageService = storageService;
            this.unitOfWork = unitOfWork;
        }
        [HttpPost]
        [ApiValidationFilter]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PostUserAsync([FromForm]CreateUserRequestDto request)
        {
            var users = _userManager.Users;
            if (await users.AnyAsync(c=>c.UserName == request.UserName))
            {
                return BadRequest(new ApiBadRequestResponse<string>("Tên người dùng không được trùng"));
            }
            if (await users.AnyAsync(c => c.PhoneNumber == request.PhoneNumber))
            {
                return BadRequest(new ApiBadRequestResponse<string>("SĐT người dùng không được trùng"));
            }
            if (await users.AnyAsync(c => c.Email == request.Email))
            {
                return BadRequest(new ApiBadRequestResponse<string>("Email người dùng không được trùng"));
            }
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber,
                FullName = request.FullName,
                Avatar = "",
                DoB = request.DoB,
                Gender = request.Gender,
                CustomId = request.CustomId,
                UserType = request.UserType,
            };
            if(request.File != null && request.File.Length >= 1 )
            {
                var filePath = $"avatar/{user.UserName}/";
                var avatar = await  SaveFileAsync(filePath, request.File);
                user.Avatar = avatar;
            }    
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                return CreatedAtAction("GetById", new { id = user.Id },request);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse<string>(result));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersAsync()
        {
            var users = _userManager.Users;

            var uservms = await users.Select(u=> _mapper.Map<UserDto>(u) ).ToListAsync();

            return Ok(new ApiResponse<List<UserDto>>(200,"Thành công",uservms));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new ApiNotFoundResponse<string>($"Không tìm thấy User với id: {id}"));

            return Ok(new ApiResponse<UserDto>(200, "Thành công", _mapper.Map<UserDto>(user)) );
        }

        [HttpPatch("{id}")]
        [ApiValidationFilter]

        public async Task<IActionResult> PutUserAsync(string id, [FromBody] UpdateUserRequestDto request)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(currentUserId != id)
            {
                var response = new ApiBadRequestResponse<string>("Thao tác không hợp lệ");
                return StatusCode(response.StatusCode,response) ;
            }    
            var user = await _userManager.FindByIdAsync(id);
            
            if (user == null)
                return NotFound(new ApiNotFoundResponse<string>($"Không thể tìm thấy người dùng với id : {id}"));
            user.PhoneNumber = request.PhoneNumber;
            user.FullName = request.FullName;
            user.DoB = request.DoB;
            user.Gender = request.Gender;
            user.Avatar = request.Avatar;
            user.UpdatedAt = DateTime.Now;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var data = _mapper.Map<UserDto>(user);
                var response = new ApiResponse<UserDto>(200, "Cập nhật thành công",data);
                return  StatusCode(response.StatusCode,response);
            }
            return BadRequest(new ApiBadRequestResponse<string>(result));
        }

        [HttpPatch("admin/{id}")]
        [ApiValidationFilter]
        [RoleRequirement(["Admin"])]
        public async Task<IActionResult> PutUserByAdminAsync(string id, [FromBody] UpdateUserByAdminRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new ApiNotFoundResponse<string>($"Không thể tìm thấy người dùng với id : {id}"));
            if (await unitOfWork.UserRepository.AnyAsync(c => c.CustomId.Equals(request.CustomId) && !c.Id.Equals(id)))
            {
                return BadRequest(new ApiBadRequestResponse<string>($"Mã bị trùng"));
            }
            user.PhoneNumber = string.IsNullOrEmpty(request.PhoneNumber) ? user.PhoneNumber : request.PhoneNumber;
            user.FullName = string.IsNullOrEmpty(request.FullName) ? user.FullName : request.FullName;
            user.DoB = request.DoB == null ? user.DoB : request.DoB ;
            user.Gender = string.IsNullOrEmpty(request.Gender) ? user.Gender : request.Gender;
            user.Avatar = string.IsNullOrEmpty(request.Avatar) ? user.Avatar : request.Avatar;
            user.CustomId = request.CustomId;  
            user.UpdatedAt = DateTime.Now;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var data = _mapper.Map<UserDto>(user);
                var response = new ApiResponse<UserDto>(200, "Cập nhật thành công", data);
                return StatusCode(response.StatusCode, response);
            }
            return BadRequest(new ApiBadRequestResponse<string>(result));
        }
        [HttpPatch("{id}/change-password")]
        public async Task<IActionResult> PutUserPasswordAsync(string id, [FromBody] ChangeUserPasswordRequestDto request)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != id)
            {
                var response = new ApiBadRequestResponse<string>("Thao tác không hợp lệ");
                return StatusCode(response.StatusCode, response);
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new ApiNotFoundResponse<string>($"Không tìm thấy người dùng với id : {id}"));

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                var data = _mapper.Map<UserDto>(user);
                var response = new ApiResponse<UserDto>(200, "Cập nhật thành công", data);
                return Ok(new ApiResponse<string>(200,"Đổi mật khẩu thành công",""));
            }
            return BadRequest(new ApiBadRequestResponse<string>(result));
        }

        [HttpDelete("{id}")]
        [ApiValidationFilter]
        [RoleRequirement(["Admin"])]
        public async Task<IActionResult> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var otherUsers = adminUsers.Where(x => x.Id != id).ToList();
            if (otherUsers.Count == 0)
            {
                return BadRequest(new ApiBadRequestResponse<string>("Bạn không thể xóa Admin duy nhất của hệ thống."));
            }
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
               return Ok(new ApiResponse<UserDto>(200,"Thành công", _mapper.Map<UserDto>(user)) );
            }
            return BadRequest(new ApiBadRequestResponse<string>(result));
        }
        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiNotFoundResponse<string>($"Không tìm thấy người dùng với id : {userId}"));
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new ApiResponse<List<string>>(200, "Thành công",roles.ToList()));
        }

        [HttpPost("{userId}/roles")]
        [RoleRequirement(["Admin"])]
        public async Task<IActionResult> PostRolesToUserUserAsync(string userId, [FromBody] RolesAssignRequestDto request)
        {
            if (request.RoleNames?.Length == 0)
            {
                return BadRequest(new ApiBadRequestResponse<string>("Vai trò không được để trống"));
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiNotFoundResponse<string>($"Không thể tìm thấy người dùng với id : {userId}"));
            var result = await _userManager.AddToRolesAsync(user, request.RoleNames);
            if (result.Succeeded)
                return Ok();

            return BadRequest(new ApiBadRequestResponse<string>(result));
        }

        [HttpDelete("{userId}/roles")]
        [RoleRequirement(["Admin"])]
        public async Task<IActionResult> RemoveRolesFromUserAsync(string userId, [FromBody] RolesAssignRequestDto request)
        {
            if (request.RoleNames?.Length == 0)
            {
                return BadRequest(new ApiBadRequestResponse<string>("Các quyền không được bỏ trống"));
            }
            if (request.RoleNames.Length == 1 && request.RoleNames[0] == "Admin")
            {
                return base.BadRequest(new ApiBadRequestResponse<string>($"Không thể gỡ bỏ quyền Admin"));
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiNotFoundResponse<string>($"Không thể tìm thấy người dùng với id : {userId}"));
            var result = await _userManager.RemoveFromRolesAsync(user, request.RoleNames);
            if (result.Succeeded)
                return Ok();

            return BadRequest(new ApiBadRequestResponse<string>(result));
        }
        
        private async Task<string> SaveFileAsync(string filePath,IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{originalFileName.Substring(0, originalFileName.LastIndexOf('.'))}{Path.GetExtension(originalFileName)}";
            var finalFilePath = filePath + fileName;

            await _storageService.SaveFileAsync(file.OpenReadStream(),filePath, originalFileName);

            return finalFilePath;
        }
    }
}
