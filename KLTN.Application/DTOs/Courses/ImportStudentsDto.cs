using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Courses
{
    public class ImportListStudentDto
    {
        public List<ImportStudent> Students { get; set; }  = new List<ImportStudent>();
    }
    public class ImportStudent
    {
        public string CustomId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email {  get; set; } = string.Empty;
        public DateTime? BirthDay { get; set; } 
        public string? PhoneNumber { get; set; }
    }
}
