﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Accounts
{
    public class LoginRequestDto
    {
        public string Identifier { get; set; }
        public string Password { get; set; }
    }
}
