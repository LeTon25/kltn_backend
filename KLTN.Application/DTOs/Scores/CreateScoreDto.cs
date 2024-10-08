using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Scores
{
    public class CreateScoreDto
    {
        public string SubmissionId { get; set; }
        public double Value { get; set; }
    }
}
