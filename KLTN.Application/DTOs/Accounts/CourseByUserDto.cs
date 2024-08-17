using KLTN.Application.DTOs.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Accounts
{
    public class CourseByUserDto
    {
        public List<CourseDto> CreatedCourses { get; set; }
        public List<CourseDto> EnrolledCourses { get; set; }
    }
}
