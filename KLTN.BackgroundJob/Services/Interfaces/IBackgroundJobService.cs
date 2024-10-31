
namespace KLTN.BackgroundJobs.Services.Interfaces
{
    public interface IBackgroundJobService
    {
        string SendReminderAssignmentDueDate(List<string> Emails,string CourseName,string AssignmentName,DateTime DueDate);
    }
}
