using KLTN.Domain.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{

    public class Brief : IDateTracking
    {
        public string Id { get; set; }
        public string Content { get; set; } 
        public string GroupId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Group? Group { get; set; }   
    }
}
