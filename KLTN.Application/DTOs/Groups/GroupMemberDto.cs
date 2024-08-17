using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Groups
{
    public class GroupMemberDto
    {
        public string StudentId { get; set; }
        public string GroupId { get; set; }
        public bool IsLeader { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string StudentName { get; set; }
    }
}
