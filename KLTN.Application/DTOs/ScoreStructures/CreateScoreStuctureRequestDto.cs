using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.ScoreStructures
{
    public class CreateScoreStuctureRequestDto
    {
        public string Id { get; set; }
        public string ColumnName { get; set; }
        public double MaxScore { get; set; }
        public double Percent { get; set; }
        public string? ParentId { get; set; }

    }
}
