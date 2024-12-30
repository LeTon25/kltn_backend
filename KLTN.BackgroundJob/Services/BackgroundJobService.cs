using KLTN.BackgroundJobs.Services.Interfaces;
using KLTN.Domain.ScheduleJobs;
using KLTN.Domain.Services;
using Microsoft.VisualBasic;

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

            var enqueueAt = DueDate.AddHours(-15);
            var jobId = ScheduleJobService.Schedule(() => _mailService.SendEmail(emailRequest, "AssignmentDeadline", placeHolders,new CancellationToken()), enqueueAt: enqueueAt);

            return jobId;
        }
        public bool DeleteJobById(string jobId)
        {
            var result = ScheduleJobService.Delete(jobId);
            return result;
        }

        public string SendNotiEvent(List<string> Emails, string CourseName,string Title ,string Message, string ObjectLink)
        {
            var emailRequest = new MailRequest()
            {
                Subject = "Thông báo",
                Body = "",
                ToAddresses = Emails,
            };
            var placeHolders = new Dictionary<string, string>()
            {
                { "CourseName" , CourseName },
                { "Title" , Title },
                { "Message" , Message },
                { "ObjectLink" , ObjectLink}
            };
            var jobId = ScheduleJobService.Enqueue(() => _mailService.SendEmail(emailRequest, "NotiEvent", placeHolders,new CancellationToken()));
            return jobId;
        }
    }
}
