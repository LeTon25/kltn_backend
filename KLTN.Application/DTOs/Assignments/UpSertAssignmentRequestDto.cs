﻿using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Uploads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Assignments
{
    public class UpSertAssignmentRequestDto
    {
        public string CourseId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime? DueDate { get; set; }
        public string[] AttachedLinks { get; set; }
        public List<FileDto> Attachments { get; set; }
        public string[] StudentAssigned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}