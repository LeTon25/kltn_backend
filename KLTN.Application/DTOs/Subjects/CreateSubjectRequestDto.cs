using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Subjects
{
    public class CreateSubjectRequestDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
