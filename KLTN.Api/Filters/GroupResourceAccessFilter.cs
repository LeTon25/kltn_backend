using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using KLTN.Application.Helpers.Response;
using static KLTN.Domain.Constants;
using KLTN.Domain;

namespace KLTN.Api.Filters
{
    public class GroupResourceAccessFilter : IAsyncAuthorizationFilter
    {
        private readonly ApplicationDbContext _db;
        public GroupResourceAccessFilter(ApplicationDbContext db) 
        {
            this._db = db;
        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var groupId = context.RouteData.Values["groupId"]?.ToString();
            var role = context.HttpContext.User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(groupId))
            {
                context.Result = new BadRequestObjectResult(new ApiBadRequestResponse<object>("Invalid group ID"));
                return;
            }

            var group =await _db.Groups
                .AsNoTracking()
                .Include(g => g.Course) 
                .FirstOrDefaultAsync(c => c.GroupId.Equals(groupId));

            if (group == null)
            {
                context.Result = new BadRequestObjectResult(new ApiBadRequestResponse<object>("Group not found"));
                return;
            }
            if (!string.IsNullOrEmpty(role) && role.Equals(Constants.Role.Admin))
            {
                return;
            }
            if (group.Course?.LecturerId == userId)
            {
                return;
            }

            if (await _db.GroupMembers.AnyAsync(c => c.StudentId == userId && c.GroupId == groupId))
            {
                return;
            }

            context.Result = new BadRequestObjectResult(new ApiBadRequestResponse<object>("Bạn không có quyền truy cập vào tài nguyên nhóm này"));
        }

    }
}