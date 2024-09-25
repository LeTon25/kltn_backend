using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using KLTN.Application.Helpers.Response;

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

            if (string.IsNullOrEmpty(groupId))
            {
                context.Result = new BadRequestObjectResult(new ApiBadRequestResponse<object>("Invalid group ID"));
                return;
            }

            var group =await _db.Groups
                .Include(g => g.Course) 
                .FirstOrDefaultAsync(c => c.GroupId.Equals(groupId));

            if (group == null)
            {
                context.Result = new BadRequestObjectResult(new ApiBadRequestResponse<object>("Group not found"));
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