using KLTN.Domain.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class Group : IDateTracking
    {
        public string GroupId { get; set; }
        public string CourseId { get; set; }
        public string GroupName { get; set; }
        public string? ProjectId { get; set; }
        public bool IsApproved { get; set; }
        public int? NumberOfMembers { get; set; }
        public DateTime CreatedAt { get ; set ; }
        public DateTime? UpdatedAt { get ; set ; }
        public DateTime? DeletedAt { get; set; }
        public Course? Course { get; set; } 
        public ICollection<GroupMember>? GroupMembers { get; set; }
    }
}
