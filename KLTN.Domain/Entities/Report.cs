using KLTN.Domain.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class Report : IDateTracking
    {
        public string ReportId { get; set; }
        public string GroupId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public List<MetaLinkData> AttachedLinks { get; set; }
        public List<File> Attachments { get; set; }
        public string[] Mentions { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Group? Group { get; set; }
        public User? CreateUser { get; set; }
        public ICollection<ReportComment>? ReportComments { get; set; }
       

    }
}
