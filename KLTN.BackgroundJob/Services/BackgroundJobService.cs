using KLTN.BackgroundJobs.Services.Interfaces;
using KLTN.Domain.ScheduleJobs;
using KLTN.Domain.Services;
using KLTN.Domain.Shared.DTOs;

namespace KLTN.BackgroundJobs.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IScheduleJobService _scheduleJobService;
        private readonly ISMTPEmailService _mailService;

        public BackgroundJobService(IScheduleJobService scheduleJobService, ISMTPEmailService mailService)
        {
            _scheduleJobService = scheduleJobService;
            _mailService = mailService;
        }

        public void SendReminderAssignmentDueDate(List<string> Emails, string CourseName, string AssignmentName, DateTime DueDate)
        {
            var placeHolders = new Dictionary<string, string>()
            {
                { "" , "" },
                { "" , "" },
                { "" , "" }
            };
        }
    }
}
