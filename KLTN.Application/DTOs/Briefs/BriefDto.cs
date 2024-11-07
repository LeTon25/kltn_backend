using KLTN.Application.DTOs.Groups;


namespace KLTN.Application.DTOs.Briefs
{
    public class BriefDto
    {
        public string Id {  get; set; } 
        public string Content { get; set; }
        public string Title { get; set; }
        public string GroupId {get; set; }
        public string? ReportId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public GroupDto? Group { get; set; }
    }
}
