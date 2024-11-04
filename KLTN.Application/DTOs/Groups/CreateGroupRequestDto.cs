
namespace KLTN.Application.DTOs.Groups
{
    public class CreateGroupRequestDto
    {
        public string GroupName { get; set; } = string.Empty;
        public string? ProjectId { get; set; }
        public string CourseId { get; set; } = string.Empty;
        public int NumberOfMembers { get; set; }
        public bool IsApproved { get; set; }
        public string GroupType { get; set; } = string.Empty;

    }
}
