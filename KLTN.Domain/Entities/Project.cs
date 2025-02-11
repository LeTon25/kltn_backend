﻿using KLTN.Domain.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class Project : IDateTracking
    {
        public string ProjectId { get; set; }
        public string CourseId { get; set; }
        public string CreateUserId { get; set; }
        public string Description { get; set; }
        public bool IsApproved {  get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Course? Course { get; set; } 
        public User? User { get; set; }
    }
}
