using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Pagination;
using KLTN.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Comments
{
    public class CommentDto
    {
        public string CommentId { get; set; }

        public string Content { get; set; }

        public string AnnouncementId { get; set; }

        public string OwnerUserId { get; set; }
        public UserDto? User { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
