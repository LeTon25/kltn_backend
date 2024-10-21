using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class Setting
    {
        public string SettingId { get; set; }
        public string CourseId { get; set; }
        public DateTime? StartGroupCreation {  get; set; }
        public DateTime? EndGroupCreation { get; set; }
        public bool AllowStudentCreateProject { get; set; }
        public bool AllowGroupRegistration { get; set; }    
        public int? MaxGroupSize { get; set; }
        public int? MinGroupSize { get; set; }
        public bool HasFinalScore { get; set; }
        public Course? Course { get; set; } 

    }
}
