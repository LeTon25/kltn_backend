using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Uploads
{
    public class FileDto
    {
        public string Url { get; set; }
        public string Name { get; set; }    
        public string Type { get; set; }   
        public FileDto(string Url,string Name,string Type) {
            this.Url = Url;
            this.Name = Name;
            this.Type = Type;
        }

    }
}
