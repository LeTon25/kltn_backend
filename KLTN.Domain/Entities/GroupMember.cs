using KLTN.Domain.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class GroupMember : IDateTracking
    {
        public Guid StudentId { get; set; }
        public Guid GroupId { get; set; }
        public bool IsLeader { get; set; }  
        public DateTime CreatedAt { get ; set; }
        public DateTime? UpdatedAt { get ; set ; }
        public DateTime? DeletedAt { get ; set; }
    }
}
