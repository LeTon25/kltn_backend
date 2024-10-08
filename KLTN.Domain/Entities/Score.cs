using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class Score
    {
        public string Id { get; set; }  
        public string ScoreStructureId { get; set; }
        public string UserId { get; set; }
        public string SubmissionId { get; set; }
        public double? Value { get; set; }
        public ScoreStructure? ScoreStructure { get; set; }
        public User? User { get; set; }
        public Submission? Submission { get; set; } 
    }
}
