using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.ScoreStructures
{
    public class StudentScoreDTO
    {
        public string Id {  get; set; }
        public string FullName { get; set; }  
        public ScoreDetailDto Scores { get; set; }  
    }
}
