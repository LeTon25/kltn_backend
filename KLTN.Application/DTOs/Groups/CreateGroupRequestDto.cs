using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Groups
{
    public class CreateGroupRequestDto
    {
        public string GroupName { get; set; }
        public string? ProjectId { get; set; }
        public string CourseId {  get; set; }

    }
}
