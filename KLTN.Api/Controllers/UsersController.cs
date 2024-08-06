using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Interfaces;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KLTN.Api.Controllers
{
    public class UsersController : BaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _uow;
        public UsersController(
           UserManager<IdentityUser> userManager,
           RoleManager<IdentityRole> roleManager,
           IUnitOfWork uow)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _uow = uow;
        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostUser(CreateUserRequestDto request)
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber,
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, request);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse(result));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = _userManager.Users;

            var uservms = await users.Select(u => new UserDto()
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
            }).ToListAsync();

            return Ok(uservms);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetUsersPaging(string filter, int pageIndex, int pageSize)
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
                .Select(u => new UserDto()
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                })
                .ToListAsync();
            var pagination = new Pagination<UserDto>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        //URL: GET: http://localhost:5001/api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Không tìm thấy User với id: {id}"));

            var userVm = new UserDto()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };
            return Ok(userVm);
        }

        [HttpPut("{id}")]
        [ApiValidationFilter]

        public async Task<IActionResult> PutUser(string id, [FromBody] CreateUserRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {id}"));

            // Tùy chỉnh các trường thông tin User sau để sau mới làm

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse(result));
        }

        [HttpPut("{id}/change-password")]
        public async Task<IActionResult> PutUserPassword(string id, [FromBody] ChangeUserPasswordRequestDto request)
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
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var otherUsers = adminUsers.Where(x => x.Id != id).ToList();
            if (otherUsers.Count == 0)
            {
                return BadRequest(new ApiBadRequestResponse("You cannot remove the only admin user remaining."));
            }
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                var uservm = new UserDto()
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                };
                return Ok(uservm);
            }
            return BadRequest(new ApiBadRequestResponse(result));
        }
        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Không tìm thấy người dùng với id : {userId}"));
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        [HttpPost("{userId}/roles")]
        public async Task<IActionResult> PostRolesToUserUser(string userId, [FromBody] RolesAssignRequestDto request)
        {
            if (request.RoleNames?.Length == 0)
            {
                return BadRequest(new ApiBadRequestResponse("Role names cannot empty"));
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {userId}"));
            var result = await _userManager.AddToRolesAsync(user, request.RoleNames);
            if (result.Succeeded)
                return Ok();

            return BadRequest(new ApiBadRequestResponse(result));
        }

        [HttpDelete("{userId}/roles")]
        public async Task<IActionResult> RemoveRolesFromUser(string userId, [FromQuery] RolesAssignRequestDto request)
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
    }
}
