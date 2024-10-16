﻿using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Projects;
using KLTN.Application.DTOs.Requests;
using KLTN.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Groups
{
    public class GroupDto
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string? ProjectId { get; set; }
        public string CourseId { get; set; }
        public int? NumberOfMembers { get; set; }
        public bool IsApproved { get; set; }
        public string? InviteCode { get; set; }
        public DateTime? InviteCodeExpired { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public ProjectDto? Project { get; set; }
        public CourseDto? Course { get; set; }
        public List<GroupMemberDto>? GroupMembers { get; set; }
        public List<RequestDto>? Requests { get; set; }
    }
}
