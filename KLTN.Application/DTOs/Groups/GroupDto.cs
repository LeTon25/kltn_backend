﻿using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Projects;
using KLTN.Application.DTOs.Requests;

namespace KLTN.Application.DTOs.Groups
{
    public class GroupDto
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string? ProjectId { get; set; }
        public string CourseId { get; set; } = string.Empty;
        public int? NumberOfMembers { get; set; }
        public bool IsApproved { get; set; }
        public string? InviteCode { get; set; }
        public string? AssignmentId { get; set; }   
        public DateTime? InviteCodeExpired { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public ProjectDto? Project { get; set; }
        public CourseDto? Course { get; set; }
        public string GroupType { get; set; } = string.Empty;
        public List<GroupMemberDto>? GroupMembers { get; set; }
        public List<RequestDto>? Requests { get; set; }
    }
}
