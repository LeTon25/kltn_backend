using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Accounts
{
    public class RefreshTokenModel
    {
        public string Token { get; set; }   
        public string RefreshToken { get; set; }
    }
}
