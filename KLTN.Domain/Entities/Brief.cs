using KLTN.Domain.Entities.Interfaces;
using System;
using System.Collections.Generic;
namespace KLTN.Domain.Entities
{

    public class Brief : IDateTracking
    {
        public string Id { get; set; }
        public string Content { get; set; } 
        public string Title { get; set; }
        public string GroupId { get; set; }
        public string? ReportId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Group? Group { get; set; }   
        public Report? Report { get; set; }
    }
}
