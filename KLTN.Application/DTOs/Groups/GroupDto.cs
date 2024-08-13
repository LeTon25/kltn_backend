using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Groups
{
    public class GroupDto
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string? ProjectId { get; set; }
        public string CourseId { get; set; }
        public int? NumberOfMembers { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string CourseGroup {  get; set; }    
    }
}
