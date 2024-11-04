using KLTN.Application.DTOs.Groups;
using KLTN.Application.DTOs.Users;

namespace KLTN.Application.DTOs.Requests
{
    public class RequestDto
    {
        public string RequestId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public UserDto? User { get; set; }
        public GroupDto? Group { get; set; }
    }
}
