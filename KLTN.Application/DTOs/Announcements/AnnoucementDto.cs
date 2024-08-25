using KLTN.Application.DTOs.Comments;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Uploads;
using KLTN.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Announcements
{
    public class AnnouncementDto
    {
        public string AnnouncementId { get; set; }
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public string Content { get; set; }
        public string[] AttachedLinks { get; set; }
        public List<FileDto> Attachments { get; set; }
        public string[] Mentions { get; set; }  
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public UserDto? CreateUser { get; set; }
        public List<CommentDto> Comments { get; set; }
        public bool IsPinned { get; set; }
    }
}
