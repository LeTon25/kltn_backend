
using KLTN.Domain.ScheduleJobs;

namespace KLTN.BackgroundJobs.Services.Interfaces
{
    public interface IBackgroundJobService
    {
        public IScheduleJobService ScheduleJobService { get; }
        string SendReminderAssignmentDueDate(List<string> Emails,string CourseName,string AssignmentName,DateTime DueDate);
    }
}
