using KLTN.Application.DTOs.Uploads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Announcements
{
    public class CreateAnnouncementRequestDto
    {
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public string[] AttachedLinks { get; set; }
        public List<FileDto> Attachments { get; set; }
        public string[] Mentions { get; set; }
        public string Content { get; set; }
        public bool IsPinned { get; set; }
    }
}
