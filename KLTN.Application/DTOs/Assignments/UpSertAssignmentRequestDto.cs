using KLTN.Application.DTOs.Groups;
using KLTN.Application.DTOs.Uploads;

namespace KLTN.Application.DTOs.Assignments
{
    public class UpSertAssignmentRequestDto
    {
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;   
        public string Content { get; set; } = string.Empty;
        public string? ScoreStructureId { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsGroupAssigned { get; set; }
        public DateTime? DueDate { get; set; }
        public List<MetaLinkDataDto> AttachedLinks { get; set; } = new List<MetaLinkDataDto>();
        public List<FileDto> Attachments { get; set; } = new List<FileDto>();
        public AutoGenerateGroupDto? AutoGenerateGroupDto { get; set; } 
    }
}
