using KLTN.Application.DTOs.Groups;
using KLTN.Application.DTOs.Users;
using KLTN.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Requests
{
    public class RequestDto
    {
        public string RequestId { get; set; }
        public string UserId { get; set; }
        public string GroupId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public UserDto? User { get; set; }
        public GroupDto? Group { get; set; }
    }
}
