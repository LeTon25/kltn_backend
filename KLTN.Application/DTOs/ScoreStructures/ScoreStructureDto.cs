using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.ScoreStructures
{
    public class ScoreStructureDto
    {
        public string Id { get; set; }
        public string ColumnName { get; set; }
        public double MaxScore { get; set; }
        public double Percent { get; set; }
        public string? ParentId { get; set; }
        public IEnumerable<ScoreStructureDto> Childrens { get; set; }
        public IEnumerable<ScoreStructureDto> Parent { get; set; }
    }
}
