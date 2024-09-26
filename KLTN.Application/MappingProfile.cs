using AutoMapper;
using KLTN.Application.DTOs.Announcements;
using KLTN.Application.DTOs.Assignments;
using KLTN.Application.DTOs.Comments;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Groups;
using KLTN.Application.DTOs.Projects;
using KLTN.Application.DTOs.Reports;
using KLTN.Application.DTOs.ScoreStructures;
using KLTN.Application.DTOs.Semesters;
using KLTN.Application.DTOs.Subjects;
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
            CreateMap<Semester,SemesterDto>();
            CreateMap<SemesterDto, Semester>();

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

        }
    }
}
