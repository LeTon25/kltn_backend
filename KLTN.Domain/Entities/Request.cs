using KLTN.Domain.Entities.Interfaces;


namespace KLTN.Domain.Entities
{
    public class Request : IDateTracking
    {
        public string RequestId { get; set; }   
        public string UserId { get; set; }  
        public string GroupId { get; set; }
        public DateTime CreatedAt { get ; set ; }
        public DateTime? UpdatedAt { get ; set ; }
        public DateTime? DeletedAt { get ; set ; }
        public User? User { get; set; }
        public Group? Group { get; set; }   
    }
}
