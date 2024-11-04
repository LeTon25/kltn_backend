using KLTN.Domain.Entities.Interfaces;

namespace KLTN.Domain.Entities
{
    public class Group : IDateTracking
    {
        public string GroupId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty; 
        public string? ProjectId { get; set; }
        public bool IsApproved { get; set; }
        public int? NumberOfMembers { get; set; }
        public string? AssignmentId { get; set; }
        public string? InviteCode { get; set; }
        public string GroupType { get; set; } = string.Empty;  
        public DateTime? InviteCodeExpired { get; set; }
        public DateTime CreatedAt { get ; set ; }
        public DateTime? UpdatedAt { get ; set ; }
        public DateTime? DeletedAt { get; set; }
        public Course? Course { get; set; } 
        public Assignment? Assignment { get; set; }
        public ICollection<GroupMember>? GroupMembers { get; set; }
        public ICollection<Report>? Reports { get; set; }
        public ICollection<Brief>? Briefs { get; set; }
        public ICollection<Request>? Requests { get; set; }
    }
}
