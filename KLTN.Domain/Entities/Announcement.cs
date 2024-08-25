using KLTN.Domain.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class Announcement : IDateTracking
    {
        public string AnnouncementId { get; set; }
        public string CourseId { get; set; }
        public string UserId { get; set; }    
        public string Content {  get; set; }
        public string[] AttachedLinks { get; set; }
        public List<File> Attachments { get; set; }
        public string[] Mentions { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
