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
        public Guid CourseId { get; set; }
        public Guid SubjectId {  get; set; }
        public Guid SemesterId { get; set; }    
        public string CourseGroup {  get; set; }
        public string? Background { get; set; }
        public string? InviteCode {  get; set; }
        public bool EnableInvite {  get; set; }
        public Guid LecturerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
