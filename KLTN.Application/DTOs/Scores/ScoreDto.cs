using KLTN.Application.DTOs.ScoreStructures;
using KLTN.Application.DTOs.Users;
using KLTN.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Scores
{
    public class ScoreDto
    {
        public string Id { get; set; }
        public string ScoreStructureId { get; set; }
        public string UserId { get; set; }
        public string SubmissionId { get; set; }
        public double? Value { get; set; }
        public UserDto? User { get; set; }
    }
}
