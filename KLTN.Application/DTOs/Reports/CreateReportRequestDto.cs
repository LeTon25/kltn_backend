using KLTN.Application.DTOs.Uploads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Reports
{
    public class CreateReportRequestDto
    {
        public string UserId { get; set; }
        public string GroupId { get; set; }
        public List<MetaLinkDataDto> AttachedLinks { get; set; }
        public List<FileDto> Attachments { get; set; }
        public string[] Mentions { get; set; }
        public string Content { get; set; }
        public bool IsPinned { get; set; }
    }
}
