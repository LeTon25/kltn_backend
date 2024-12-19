
using KLTN.Domain.ScheduleJobs;

namespace KLTN.BackgroundJobs.Services.Interfaces
{
    public interface IBackgroundJobService
    {
        public IScheduleJobService ScheduleJobService { get; }
        string SendReminderAssignmentDueDate(List<string> Emails,string CourseName,string AssignmentName,DateTime DueDate);
        //string SendNotiEvent(List<string> Emails, string CourseName, string Message, string ObjectLink);
    }
}
