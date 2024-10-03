using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Groups
{
    public class GroupInviteCodeDto
    {
        public string? InviteCode { get; set; }
        public DateTime? InviteCodeExpired { get; set; }
    }
}
