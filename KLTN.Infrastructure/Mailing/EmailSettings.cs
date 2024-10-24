﻿using KLTN.Domain.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Infrastructure.Mailing
{
    public class SMTPEmailSettings : ISMTPEmailSettings
    {
        public string DisplayName { get; set; }
        public bool EnableVerification { get; set; }
        public string From { get; set; }
        public bool UseSsl { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Host { get ; set ; }
        public string TemplateFolderPath { get; set; }
    }
}
