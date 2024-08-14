using KLTN.Domain.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class EnrolledCourse : IDateTracking
    {
        public string StudentId { get; set; } 
        public string CourseId { get; set; }
        public DateTime CreatedAt { get ; set; }
        public DateTime? UpdatedAt { get ; set ; }
        public DateTime? DeletedAt { get; set; }
    }
}
