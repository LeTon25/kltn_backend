using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Groups
{
    public class AddMemberToGroupDto
    {
        public string[] studentIds { get; set; }    
        public string leaderId { get; set; }    
    }
}
