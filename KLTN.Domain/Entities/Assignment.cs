using KLTN.Domain.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class Assignment : IDateTracking
    {
        public string AssignmentId { get; set; }    
        public string CourseId {get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime? DueDate { get; set; }
        public List<MetaLinkData> AttachedLinks { get; set; }
        public List<File> Attachments { get; set; }
        public string[] StudentAssigned { get;set; }
        public DateTime CreatedAt { get ; set ; }
        public DateTime? UpdatedAt { get ; set ; }
        public DateTime? DeletedAt { get ; set; }
    }
}
