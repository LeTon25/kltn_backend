using KLTN.Application.DTOs.Uploads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Assignments
{
    public class FileInAssignmentDto
    {
        public Dictionary<string, List<FileDto>> Files = new Dictionary<string, List<FileDto>>();
    }
}
