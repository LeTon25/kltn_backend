using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Courses
{
    public class CreateCourseRequestDto
    {
        public string SubjectId { get; set; }
        public string Name { get; set; }
        public string CourseGroup { get; set; }
        public string? Background { get; set; }
        public string? InviteCode { get; set; }
        public bool EnableInvite { get; set; } = true;
        public string LecturerId { get; set; }
        public string Semester { get; set; }
        public bool IsHidden { get; set; } = false;

    }
}
