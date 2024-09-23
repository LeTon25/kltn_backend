using KLTN.Domain.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class Subject : IDateTracking
    {
        public  string SubjectId { get; set; }
        public string SubjectCode { get; set; }
        public string Name { get; set; } 
        public string? Description { get; set; }
        public DateTime CreatedAt { get ; set ; }
        public DateTime? UpdatedAt { get ; set; }
        public DateTime? DeletedAt { get; set; }
        public ICollection<Course>? Courses { get; set; }
    }
}
