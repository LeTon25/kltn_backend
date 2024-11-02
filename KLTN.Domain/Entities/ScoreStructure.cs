using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class ScoreStructure
    {
        public string Id { get; set; }  
        public string ColumnName { get; set; }  
        public double Percent {  get; set; }
        public string? CourseId { get; set; }    
        public string? ParentId { get; set; }
        public ScoreStructure? Parent { get; set; } 
        public ICollection<ScoreStructure>? Children { get;set; }
        public ICollection<Score> Scores { get; set; }
        public Assignment? Assignment { get; set; }
    }
}
