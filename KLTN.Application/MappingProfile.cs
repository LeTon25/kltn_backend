using AutoMapper;
using KLTN.Application.DTOs.Announcements;
using KLTN.Application.DTOs.Assignments;
using KLTN.Application.DTOs.Briefs;
using KLTN.Application.DTOs.Comments;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Groups;
using KLTN.Application.DTOs.Projects;
using KLTN.Application.DTOs.Reports;
using KLTN.Application.DTOs.Requests;
using KLTN.Application.DTOs.Scores;
using KLTN.Application.DTOs.ScoreStructures;
using KLTN.Application.DTOs.Settings;
using KLTN.Application.DTOs.Subjects;
using KLTN.Application.DTOs.Submissions;
using KLTN.Application.DTOs.Uploads;
using KLTN.Application.DTOs.Users;
using KLTN.Domain.Entities;

using File = KLTN.Domain.Entities.File;

namespace KLTN.Application
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {

            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();

            CreateMap<Announcement, AnnouncementDto>();
            CreateMap<AnnouncementDto, Announcement>();

            CreateMap<Course, CourseDto>().ReverseMap();

            CreateMap<Project, ProjectDto>();

            CreateMap<Group,GroupDto>().ReverseMap();

            CreateMap<Subject,SubjectDto>().ReverseMap();


            CreateMap<File, FileDto>();
            CreateMap<FileDto, File>();
            
            CreateMap<Comment, CommentDto>().ReverseMap();

            CreateMap<AssignmentDto, Assignment>().ReverseMap();
            CreateMap<AssignmentNoCourseDto,Assignment>().ReverseMap();

            CreateMap<ScoreStructure,ScoreStructureDto>().ReverseMap();
            CreateMap<UpSertScoreStructureDto, ScoreStructure>();

            CreateMap<MetaLinkData, MetaLinkDataDto>().ReverseMap();

            CreateMap<ReportDto,Report>().ReverseMap();
            CreateMap<CreateReportRequestDto, Report>();

            CreateMap<SubmissionDto,Submission>().ReverseMap();

            CreateMap<ScoreDto,Score>().ReverseMap();
            CreateMap<SubmissionNoScoreDto, Submission>().ReverseMap();
            CreateMap<Brief,BriefDto>().ReverseMap();

            CreateMap<Request,RequestDto>().ReverseMap();
            CreateMap<GroupMember, GroupMemberDto>().ReverseMap();  

            CreateMap<Setting,SettingDto>().ReverseMap();
        }
    }
}
