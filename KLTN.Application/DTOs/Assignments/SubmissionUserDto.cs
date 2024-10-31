using KLTN.Application.DTOs.Scores;
using KLTN.Application.DTOs.Submissions;
using KLTN.Application.DTOs.Users;


namespace KLTN.Application.DTOs.Assignments
{
    public class SubmissionUserDto
    {
        public UserDto User { get; set; }
        public SubmissionNoScoreDto? Submission { get; set; }
        public double? Score { get; set; }   
        public string? GroupId { get; set; } = string.Empty;
        public string? GroupName { get; set; } = string.Empty;
        

    }
}
