using KLTN.Domain.Entities.Interfaces;

namespace KLTN.Domain.Entities
{
    public class Submission : IDateTracking
    {
        public string SubmissionId { get; set; }    
        public string Description { get; set; }
        public string AssignmentId { get; set; }
        public string UserId { get; set; }
        public List<MetaLinkData> AttachedLinks { get; set; }
        public List<File> Attachments { get; set; }
        public DateTime CreatedAt { get ; set ; }
        public DateTime? UpdatedAt { get ; set ; }
        public DateTime? DeletedAt { get ; set ; }
        public Assignment? Assignment { get; set; }  
        public User? CreateUser { get; set; }
    }
}
