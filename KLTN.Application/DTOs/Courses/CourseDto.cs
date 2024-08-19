using KLTN.Application.DTOs.Semesters;
using KLTN.Application.DTOs.Subjects;
using KLTN.Application.DTOs.Users;
using KLTN.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Courses
{
    public class CourseDto
    {
        public string CourseId { get; set; }
        public string SubjectId { get; set; }
        public string SemesterId { get; set; }
        public string CourseGroup { get; set; }
        public string? Background { get; set; }
        public string? InviteCode { get; set; }
        public bool EnableInvite { get; set; }
        public string LecturerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public SubjectDto? Subject { get; set; }   
        public UserDto? Lecturer { get ; set; }    
        public SemesterDto? Semester { get; set; } 

    }
}
