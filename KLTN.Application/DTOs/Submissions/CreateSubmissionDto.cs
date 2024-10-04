using KLTN.Application.DTOs.Uploads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Submissions
{
    public class CreateSubmissionDto
    {
        public string Description { get; set; }
        public List<MetaLinkDataDto> AttachedLinks { get; set; }
        public List<FileDto> Attachments { get; set; }
    }
}
