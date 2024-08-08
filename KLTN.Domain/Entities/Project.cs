using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class Project
    {
        public Guid ProjectId { get; set; }
        public Guid SubjectId { get; set; }
        public Guid CreateUserId { get; set; }
        public Guid GroupId { get; set; }   
        public string Description { get; set; }
        public Guid CourseId { get; set; }
        public bool IsApproved {  get; set; }
        public string Title { get; set; }
    }
}
