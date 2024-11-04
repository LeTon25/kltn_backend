using KLTN.Application.DTOs.Announcements;
using KLTN.Application.DTOs.Assignments;
using KLTN.Application.DTOs.ScoreStructures;
using KLTN.Application.DTOs.Settings;
using KLTN.Application.DTOs.Subjects;
using KLTN.Application.DTOs.Users;


namespace KLTN.Application.DTOs.Courses
{
    public class CourseDto
    {
        public string CourseId { get; set; }
        public string SubjectId { get; set; }
        public string SemesterId { get; set; }
        public string CourseGroup { get; set; }
        public string Name { get; set; }
        public string Semester { get; set; }
        public string? Background { get; set; }
        public string? InviteCode { get; set; }
        public bool EnableInvite { get; set; }
        public string LecturerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? SaveAt { get; set; }
        public bool IsHidden { get; set; } = false;

        public SubjectDto? Subject { get; set; }   
        public UserDto? Lecturer { get ; set; }    
        public List<UserDto>? Students {get; set; }
        public ScoreStructureDto? ScoreStructure { get; set; }
        public List<AnnouncementDto>? Announcements { get; set; }
        public List<AssignmentNoCourseDto>? Assignments { get; set; }  
        public SettingDto? Setting { get; set; }    

    }
}
