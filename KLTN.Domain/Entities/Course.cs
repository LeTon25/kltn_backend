using KLTN.Domain.Entities.Interfaces;
namespace KLTN.Domain.Entities
{
    public class Course : IDateTracking
    {
        public string CourseId { get; set; }
        public string SubjectId {  get; set; }
        public string Name { get; set; }
        public string CourseGroup {  get; set; }
        public string? Background { get; set; }
        public string? InviteCode {  get; set; }
        public bool EnableInvite {  get; set; }
        public string LecturerId { get; set; }
        public string Semester { get; set; }  
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? SaveAt { get; set; }
        public bool IsHidden { get; set; }
        public ICollection<Assignment>? Assignments { get; set; }
        public ICollection<Announcement>? Annoucements { get; set; }
        public ICollection<Group>? Groups { get; set; }
        public ICollection<Project>? Projects { get; set; }
        public Subject? Subject { get; set; }   
        public User? Lecturer { get; set; }
        public ICollection<EnrolledCourse>? EnrolledCourses { get; set; }
        public Setting? Setting { get; set; }   
    }
}
