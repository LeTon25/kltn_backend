using KLTN.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Users
{
    public class UpdateUserRequestDto
    {
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public DateTime? DoB { get; set; }
        public bool Gender { get; set; }
        public string Avatar { get; set; }
    }
}
