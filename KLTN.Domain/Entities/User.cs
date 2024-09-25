using KLTN.Domain.Entities.Interfaces;
using KLTN.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class User : IdentityUser,IDateTracking
    {
        public string FullName { get; set; }
        public DateTime? DoB { get; set; }
        public bool Gender { get; set; }
        public string? CustomId { get; set; }   
        public string? Avatar { get; set; }
        public UserType UserType { get; set; }
        public DateTime CreatedAt { get ; set ; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public ICollection<Announcement>? Announcements { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<Course>? CreatedCourses { get; set; } 
        public ICollection<EnrolledCourse>? EnrolledCourses { get; set; }   
        public ICollection<GroupMember>? GroupMembers { get; set; }
        public ICollection<Report>? Reports { get; set; }   
        public ICollection<ReportComment>? ReportComments { get; set; }

    }
}
