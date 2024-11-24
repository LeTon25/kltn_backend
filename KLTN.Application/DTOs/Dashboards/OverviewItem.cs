using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Dashboards
{
    
    public class OverviewItem
    {
        public string Month { get; set; } = string.Empty;
        public int Courses {  get; set; }     
        public int Users { get; set; }
        public int Subjects { get; set; }   
    }
}
