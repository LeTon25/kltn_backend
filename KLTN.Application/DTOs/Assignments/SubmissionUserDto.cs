using KLTN.Application.DTOs.Scores;
using KLTN.Application.DTOs.Submissions;
using KLTN.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Assignments
{
    public class SubmissionUserDto
    {
        public UserDto User { get; set; }
        public SubmissionNoScoreDto? Submission { get; set; }
        public double? Score { get; set; }   

    }
}
