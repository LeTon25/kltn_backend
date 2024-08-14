using AutoMapper;
using FluentValidation.Validators;
using KLTN.Api.Services.Interfaces;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Net.Http.Headers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KLTN.Api.Controllers
{
    public class UsersController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;
        public UsersController(
           UserManager<User> userManager,
           RoleManager<IdentityRole> roleManager,
           IMapper mapper,
           IStorageService storageService
           )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _storageService = storageService;
        }
        [HttpPost]
        [ApiValidationFilter]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PostUserAsync([FromForm]CreateUserRequestDto request)
        {
            var users = _userManager.Users;
            if (await users.AnyAsync(c=>c.UserName == request.UserName))
            {
                return BadRequest(new ApiBadRequestResponse("Tên người dùng không được trùng"));
            }
            if (await users.AnyAsync(c => c.PhoneNumber == request.PhoneNumber))
            {
                return BadRequest(new ApiBadRequestResponse("SĐT người dùng không được trùng"));
            }
            if (await users.AnyAsync(c => c.Email == request.Email))
            {
                return BadRequest(new ApiBadRequestResponse("Email người dùng không được trùng"));
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
                return BadRequest(new ApiBadRequestResponse(result));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersAsync()
        {
            var users = _userManager.Users;

            var uservms = await users.Select(u=> _mapper.Map<UserDto>(u)).ToListAsync();

            return Ok(uservms);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetUsersPagingAsync(string filter, int pageIndex, int pageSize)
        {
            var query = _userManager.Users;
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Email.Contains(filter)
                || x.UserName.Contains(filter)
                || x.PhoneNumber.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(u =>_mapper.Map<UserDto>(u))
                .ToListAsync();
            var pagination = new Pagination<UserDto>
            {
                Items = items,
                TotalRecords = totalRecords,
                PageSize = pageSize,
                PageIndex = pageIndex
            };
            return Ok(pagination);
        }

        //URL: GET: http://localhost:5001/api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Không tìm thấy User với id: {id}"));

            return Ok(_mapper.Map<UserDto>(user));
        }

        [HttpPut("{id}")]
        [ApiValidationFilter]

        public async Task<IActionResult> PutUserAsync(string id, [FromBody] CreateUserRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Không thể tìm thấy người dùng với id : {id}"));

            // Tùy chỉnh các trường thông tin User sau để sau mới làm
            user.PhoneNumber = request.PhoneNumber;
            user.FullName = request.FullName;
            user.DoB = request.DoB;
            user.Gender = request.Gender;
            user.CustomId = request.CustomId;

            if (request.File != null && request.File.Length >=1)
            {
                if(!string.IsNullOrEmpty(user.Avatar))
                {
                    await _storageService.DeleteFileAsync(user.Avatar);
                }
                var filePath = $"avatar/{user.UserName}/";
                var avatar = await SaveFileAsync(filePath, request.File);
                user.Avatar = avatar;
            }
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse(result));
        }

        [HttpPut("{id}/change-password")]
        public async Task<IActionResult> PutUserPasswordAsync(string id, [FromBody] ChangeUserPasswordRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Không tìm thấy người dùng với id : {id}"));

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse(result));
        }

        [HttpDelete("{id}")]
        [ApiValidationFilter]
        public async Task<IActionResult> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var otherUsers = adminUsers.Where(x => x.Id != id).ToList();
            if (otherUsers.Count == 0)
            {
                return BadRequest(new ApiBadRequestResponse("Bạn không thể xóa Admin duy nhất của hệ thống."));
            }
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
               return Ok(_mapper.Map<UserDto>(user));
            }
            return BadRequest(new ApiBadRequestResponse(result));
        }
        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Không tìm thấy người dùng với id : {userId}"));
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        [HttpPost("{userId}/roles")]
        public async Task<IActionResult> PostRolesToUserUserAsync(string userId, [FromBody] RolesAssignRequestDto request)
        {
            if (request.RoleNames?.Length == 0)
            {
                return BadRequest(new ApiBadRequestResponse("Vai trò không được để trống"));
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Không thể tìm thấy người dùng với id : {userId}"));
            var result = await _userManager.AddToRolesAsync(user, request.RoleNames);
            if (result.Succeeded)
                return Ok();

            return BadRequest(new ApiBadRequestResponse(result));
        }

        [HttpDelete("{userId}/roles")]
        public async Task<IActionResult> RemoveRolesFromUserAsync(string userId, [FromBody] RolesAssignRequestDto request)
        {
            if (request.RoleNames?.Length == 0)
            {
                return BadRequest(new ApiBadRequestResponse("Các quyền không được bỏ trống"));
            }
            if (request.RoleNames.Length == 1 && request.RoleNames[0] == "Admin")
            {
                return base.BadRequest(new ApiBadRequestResponse($"Không thể gỡ bỏ quyền Admin"));
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Không thể tìm thấy người dùng với id : {userId}"));
            var result = await _userManager.RemoveFromRolesAsync(user, request.RoleNames);
            if (result.Succeeded)
                return Ok();

            return BadRequest(new ApiBadRequestResponse(result));
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
