using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Shared.DTOs
{
    public class NotiEventDto
    {
        public List<string> Emails { get; set; } = new List<string>();
        public string CourseName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty ;
        public string Title {  get; set; } = string.Empty ;
        public string ObjectLink { get; set; } = string.Empty;
    }
}
