using KLTN.Domain.Entities.Interfaces;

namespace KLTN.Domain.Entities
{
    public class Assignment : IDateTracking
    {
        public string AssignmentId { get; set; }    
        public string CourseId {get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? ScoreStructureId { get; set; }
        public bool IsGroupAssigned {  get; set; }
        public bool IsIndividualSubmissionRequired { get; set; }
        public string Type { get; set; }
        public string? JobId { get; set; }   
        public DateTime? DueDate { get; set; }
        public List<MetaLinkData> AttachedLinks { get; set; }
        public List<File> Attachments { get; set; }
        public DateTime CreatedAt { get ; set ; }
        public DateTime? UpdatedAt { get ; set ; }
        public DateTime? DeletedAt { get ; set; }
        public Course? Course { get; set; }
        public ICollection<Submission>? Submissions { get; set; }
        public ScoreStructure? ScoreStructure { get; set; }
        public ICollection<Group>? Groups { get; set; }
    }
}
