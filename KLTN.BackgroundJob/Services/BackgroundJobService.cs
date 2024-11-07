using KLTN.BackgroundJobs.Services.Interfaces;
using KLTN.Domain.ScheduleJobs;
using KLTN.Domain.Services;

namespace KLTN.BackgroundJobs.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly ISMTPEmailService _mailService;

        public IScheduleJobService ScheduleJobService { get; private set; }

        public BackgroundJobService(IScheduleJobService scheduleJobService, ISMTPEmailService mailService)
        {
            ScheduleJobService = scheduleJobService;
            _mailService = mailService;
        }
        public string SendReminderAssignmentDueDate(List<string> Emails, string CourseName, string AssignmentName, DateTime DueDate)
        {
            var emailRequest = new MailRequest()
            {
                Subject = "Thông báo",
                Body = "",
                ToAddresses = Emails,
            };
            var placeHolders = new Dictionary<string, string>()
            {
                { "ClassName" , CourseName },
                { "AssignmentTitle" , AssignmentName },
                { "DueDate" , DueDate.ToString("dd-MM-yyyy HH:mm:ss") }
            };

            var jobId = ScheduleJobService.Schedule(() => _mailService.SendEmail(emailRequest, "AssignmentDeadline", placeHolders,new CancellationToken()), enqueueAt: DueDate.AddHours(-8));

            return jobId;
        }
        public bool DeleteJobById(string jobId)
        {
            var result = ScheduleJobService.Delete(jobId);
            return result;
        }
    }
}
