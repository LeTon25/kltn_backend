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
        public Guid CommentId { get; set; }
        public Guid AnnoucementId { get; set; }
        public Guid UserId { get; set; }
        public string Content {  get; set; }
        public DateTime CreatedAt { get ; set ; }
        public DateTime? UpdatedAt { get ; set ; }
        public DateTime? DeletedAt { get; set; }
    }
}
