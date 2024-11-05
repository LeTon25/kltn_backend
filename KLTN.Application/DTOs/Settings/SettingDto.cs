namespace KLTN.Application.DTOs.Settings
{
    public class SettingDto
    {
        public string SettingId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public DateTime? DueDateToJoinGroup { get; set; }
        public bool AllowStudentCreateProject { get; set; }
        public int? MaxGroupSize { get; set; }
        public int? MinGroupSize { get; set; }
        public bool HasFinalScore { get; set; }
    }
}
