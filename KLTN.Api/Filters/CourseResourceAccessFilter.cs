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
            var courseId = context.RouteData.Values["courseId"].ToString();

            var course = await _db.Courses.FirstOrDefaultAsync(c=>c.CourseId == courseId);
            if (course.LecturerId == userId)
            {
                return;
            }
            if(await _db.EnrolledCourse.AnyAsync(c=>c.StudentId == userId && c.CourseId == courseId))
            {
                return;
            }    
            context.Result = new BadRequestObjectResult(new ApiBadRequestResponse<object>("Bạn không có quyền truy cập vào tài nguyên lớp này"));
        }
    }
}