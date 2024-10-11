using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.ScoreStructures
{
    public class ScoreDetailDto
    {
        public string ScoreStructureId { get; set; }  
        public string ColumnName { get; set; }        
        public double? Value { get; set; }
        public double Percent {  get; set; }
        public string? ScoreId { get; set; }
        public List<ScoreDetailDto> Children { get; set; } = new List<ScoreDetailDto>();  // Điểm của các cột con
    }
}
