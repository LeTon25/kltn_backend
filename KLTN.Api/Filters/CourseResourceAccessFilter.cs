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
using System.Reflection.Metadata;
using KLTN.Domain;

namespace KLTN.Api.Filters
{
    public class CourseResourceAccessFilter : IAsyncAuthorizationFilter
    {
        private readonly ApplicationDbContext _db;
        public CourseResourceAccessFilter(ApplicationDbContext db) 
        {
            this._db = db;
        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var courseId = context.RouteData.Values["courseId"]?.ToString();
            var role = context.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(courseId))
            {
                context.Result = new BadRequestObjectResult(new ApiBadRequestResponse<object>("Invalid course ID"));
                return;
            }

            var course = await _db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.CourseId == courseId);

            if(!string.IsNullOrEmpty(role ) && role.Equals(Constants.Role.Admin))
            {
                return;
            }    
            if (course == null)
            {
                context.Result = new BadRequestObjectResult(new ApiBadRequestResponse<object>("Course not found"));
                return;
            }

            if (course.LecturerId == userId)
            {
                return;
            }

            if (await _db.EnrolledCourse.AnyAsync(c => c.StudentId == userId && c.CourseId == courseId))
            {
                return;
            }

            context.Result = new BadRequestObjectResult(new ApiBadRequestResponse<object>("Bạn không có quyền truy cập vào tài nguyên lớp này"));
        }

    }
}