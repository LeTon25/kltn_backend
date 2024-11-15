using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.ScoreStructures
{
    public class TranscriptStatisticsDto
    {
        public List<ColumnStatistics> ColumnStatistics { get; set; } = new List<ColumnStatistics>();
    }
    public class ColumnStatistics
    {
        public string ColumnName { get; set; }
        public double Average { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
        public Dictionary<string, int> Distribution { get; set; } = new Dictionary<string, int>();
    }
}
