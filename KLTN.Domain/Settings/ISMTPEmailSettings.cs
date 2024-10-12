using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Settings
{
    public interface ISMTPEmailSettings
    {
        public string DisplayName { get; set; }
        public bool EnableVerification { get;set; }
        public string From { get; set; }    
        public bool UseSsl { get; set; }
        public int Port { get; set; }   
        public string Host {  get; set; }
        public string UserName { get; set; }    
        public string Password { get; set; }
    }
}
