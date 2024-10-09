using KLTN.Application.DTOs.Scores;
using KLTN.Application.DTOs.Uploads;
using KLTN.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Submissions
{
    public class SubmissionNoScoreDto
    {
        public string SubmissionId { get; set; }
        public string Description { get; set; }
        public string AssignmentId { get; set; }
        public string UserId { get; set; }
        public List<MetaLinkDataDto> AttachedLinks { get; set; }
        public List<FileDto> Attachments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public UserDto? CreateUser { get; set; }
    }
}
