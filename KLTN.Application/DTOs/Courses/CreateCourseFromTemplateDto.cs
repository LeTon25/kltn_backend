using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Courses
{
    public class CreateCourseFromTemplateDto
    {
        public string CourseGroup { get; set; } = string.Empty;
        public string SourceCourseId { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
