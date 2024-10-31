
namespace KLTN.BackgroundJobs.Services.Interfaces
{
    public interface IBackgroundJobService
    {
        void SendReminderAssignmentDueDate(List<string> Emails,string CourseName,string AssignmentName,DateTime DueDate);
    }
}
