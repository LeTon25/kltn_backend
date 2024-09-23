using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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
        public List<ScoreStructureDto> Children { get; set; }
        [JsonIgnore]
        public ScoreStructureDto Parent { get; set; }
    }
}
