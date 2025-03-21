﻿namespace KLTN.Application.DTOs.ScoreStructures
{
    public class UpSertScoreStructureDto
    {
        public string Id { get; set; }  
        public string ColumnName { get; set; }
        public double Percent { get; set; }
        public string? CourseId { get; set; }
        public string? ParentId { get; set; }
        public string? divideColumnFirst { get; set; }
        public string? divideColumnSecond { get; set; }
        public List<ScoreStructureDto>? Children { get; set; }
    }
}
