using KLTN.Domain.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Entities
{
    public class Comment : IDateTracking
    {
        public string CommentId { get; set; }
        public string AnnoucementId { get; set; }
        public string UserId { get; set; }
        public string Content {  get; set; }
        public DateTime CreatedAt { get ; set ; }
        public DateTime? UpdatedAt { get ; set ; }
        public DateTime? DeletedAt { get; set; }
    }
}
