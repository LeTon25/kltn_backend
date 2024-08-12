using KLTN.Domain.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class Course : IDateTracking
    {
        public string CourseId { get; set; }
        public string SubjectId {  get; set; }
        public string SemesterId { get; set; }    
        public string CourseGroup {  get; set; }
        public string? Background { get; set; }
        public string? InviteCode {  get; set; }
        public bool EnableInvite {  get; set; }
        public string LecturerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
