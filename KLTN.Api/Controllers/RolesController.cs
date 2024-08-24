using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Infrastructure.Data;
using KLTN.Application.DTOs.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class RolesController : BaseController
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _roleManager = roleManager;
        }

        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostRoleAsync(CreateRoleRequestDto request)
        {
            var role = new IdentityRole()
            {
                Name = request.Name,
                NormalizedName = request.Name.ToUpper()
            };
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return CreatedAtAction("GetById", new { id = role.Id }, request);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse<string>(result));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRolesAsync()
        {
            var roles = _roleManager.Roles;

            var rolevms = await roles.Select(r => new RoleDto()
            {
                Id = r.Id,
                Name = r.Name
            }).ToListAsync();

            return Ok(new ApiResponse<List<RoleDto>>(200,"Thành công",rolevms));
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetRolesPagingAsync(string filter, int pageIndex, int pageSize)
        {
            var query = _roleManager.Roles;
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Id.Contains(filter) || x.Name.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RoleDto()
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .ToListAsync();

            var pagination = new Pagination<RoleDto>
            {
                Items = items,
                TotalRecords = totalRecords,
                PageSize = pageSize,
                PageIndex = pageIndex
            };
            return Ok(new ApiResponse<Pagination<RoleDto>>(200,"Thành công",pagination));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound(new ApiNotFoundResponse<string>($"Không thể tìm thấy Role với id : {id}"));

            var roleVm = new RoleDto()
            {
                Id = role.Id,
                Name = role.Name,
            };
            return Ok(new ApiResponse<RoleDto>(200, "Thành công",roleVm));
        }

        [HttpPut("{id}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutRoleAsync(string id, [FromBody] CreateRoleRequestDto roleVm)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound(new ApiNotFoundResponse<string>($"Không tìm thấy role với id : {id}"));

            role.Name = roleVm.Name;
            role.NormalizedName = roleVm.Name.ToUpper();

            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return Ok(new ApiResponse<string>(200,"Thành công"));
            }
            return BadRequest(new ApiBadRequestResponse<string>(result));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoleAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound(new ApiNotFoundResponse<string>($"Không tìm thấy role với id : {id}"));

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                var rolevm = new RoleDto()
                {
                    Id = role.Id,
                    Name = role.Name
                };
                return Ok(new ApiResponse<RoleDto>(200,"Xóa thành công",rolevm));
            }
            return BadRequest(new ApiBadRequestResponse<string>(result));
        }
    }
}
