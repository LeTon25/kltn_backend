namespace KLTN.Domain.Shared.DTOs
{
    public class AssignmentReminderDto
    {
        public List<string> Emails { get; set; } = new List<string>();
        public string CourseName { get; set; } = string.Empty;
        public string AssignmentTitle { get; set; } = string.Empty;
        public DateTime DueDate { get; set; } = DateTime.Now;

    }
}
