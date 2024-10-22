namespace KLTN.Application.DTOs.Projects
{
    public class CreateProjectRequestDto
    {
        public string CourseId { get; set; }
        public string CreateUserId { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public bool IsApproved {  get; set; }   
    }
}
