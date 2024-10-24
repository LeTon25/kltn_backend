﻿using KLTN.Application.DTOs.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Briefs
{
    public class BriefDto
    {
        public string Id {  get; set; } 
        public string Content { get; set; }
        public string Title { get; set; }
        public string GroupId {get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public GroupDto? Group { get; set; }

    }
}
