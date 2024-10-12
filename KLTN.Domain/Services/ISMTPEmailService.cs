using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KLTN.Domain.Services
{
    public class MailRequest
    {
        [EmailAddress]
        public string From { get; set; }
        [EmailAddress]
        public string ToAddress { get; set; }
        public IEnumerable<string> ToAddresses { get; set; }  = new List<string>();
        public string Subject { get; set; }
        public string Body { get; set; }    
        public IFormFileCollection Attachments { get; set; }

    }
    public interface ISMTPEmailService : IEmailService<MailRequest> { }
}
