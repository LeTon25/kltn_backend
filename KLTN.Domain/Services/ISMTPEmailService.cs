using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KLTN.Domain.Services
{
    public class MailRequest
    {
        [EmailAddress]
        public string From { get; set; } = string.Empty;
        [EmailAddress]
        public string ToAddress { get; set; } = string.Empty;
        public IEnumerable<string> ToAddresses { get; set; }  = new List<string>();
        public string Subject { get; set; } = string.Empty ;
        public string Body { get; set; } = string.Empty;
        //public IFormFileCollection Attachments { get; set; }
    }
    public interface ISMTPEmailService : IEmailService<MailRequest> { }
}
