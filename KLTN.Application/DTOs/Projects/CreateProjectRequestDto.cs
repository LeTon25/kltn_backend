using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Projects
{
    public class CreateProjectRequestDto
    {
        public string SubjectId { get; set; }
        public string CreateUserId { get; set; }
        public string GroupId { get; set; }
        public string Description { get; set; }
        public bool IsApproved { get; set; }
        public string Title { get; set; }
    }
}
