using KLTN.Domain.Entities.Interfaces;

namespace KLTN.Domain.Entities
{
    public class GroupMember : IDateTracking
    {
        public string StudentId { get; set; }
        public string GroupId { get; set; }
        public bool IsLeader { get; set; }  
        public DateTime CreatedAt { get ; set; }
        public DateTime? UpdatedAt { get ; set ; }
        public DateTime? DeletedAt { get ; set; }
        public User? Member {  get; set; }  
        public Group? Group { get; set; }
    }
}
