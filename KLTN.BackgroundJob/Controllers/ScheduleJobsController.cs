using KLTN.BackgroundJobs.Services.Interfaces;
using KLTN.Domain.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace KLTN.BackgroundJobs.Controllers
{
    [Route("api/schedule-jobs")]
    [ApiController]
    public class ScheduleJobsController : ControllerBase
    {
        private readonly IBackgroundJobService backgroundJobService;
        public ScheduleJobsController(IBackgroundJobService backgroundJobService)
        {
            this.backgroundJobService = backgroundJobService;
        }
        [HttpPost("assignment-due-date")]
        public IActionResult SendReminerAssignmentDueDate(AssignmentReminderDto dto)
        {
            var JobId = backgroundJobService.SendReminderAssignmentDueDate(dto.Emails, dto.CourseName, dto.AssignmentTitle, dto.DueDate);
            return Ok(JobId);
        }
        [HttpPost("send-noti")]
        public IActionResult SendNoti(NotiEventDto dto)
        {
            var JobId = backgroundJobService.SendNotiEvent(dto.Emails, dto.CourseName, dto.Title, dto.Message, dto.ObjectLink);
            return Ok(JobId);
        }
        [HttpDelete("{jobId}")]
        public IActionResult DeleteJob(string jobId) 
        {
            var result = backgroundJobService.ScheduleJobService.Delete(jobId);
            return Ok(result);
        } 
        
    }
}
