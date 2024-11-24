using KLTN.Application.DTOs.Users;
using KLTN.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Accounts
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }    
        public DateTime TokenExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
        public string Role { get; set; }
        public UserDto User { get; set; }
    }
}
