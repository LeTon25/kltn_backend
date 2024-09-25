using KLTN.Domain.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class ReportComment : IDateTracking
    {
        public string ReportCommentId { get; set; }
        public string ReportId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Report? Report { get; set; }
        public User? User { get; set; }

    }
}
