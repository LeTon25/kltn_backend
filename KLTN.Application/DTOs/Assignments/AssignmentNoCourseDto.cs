using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.ScoreStructures;
using KLTN.Application.DTOs.Uploads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Assignments
{
    public class AssignmentNoCourseDto
    {
        public string AssignmentId { get; set; }
        public string CourseId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }

        public bool IsGroupAssigned { get; set; }
        public string? ScoreStructureId { get; set; }
        public DateTime? DueDate { get; set; }
        public List<MetaLinkDataDto> AttachedLinks { get; set; }
        public List<FileDto> Attachments { get; set; }
        public string[] StudentAssigned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public ScoreStructureDto? ScoreStructure { get; set; }

    }
}
