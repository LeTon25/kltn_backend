using AutoMapper;
using KLTN.Application.DTOs.Announcements;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Groups;
using KLTN.Application.DTOs.Projects;
using KLTN.Application.DTOs.Semesters;
using KLTN.Application.DTOs.Subjects;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Domain;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class CoursesController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public CoursesController(ApplicationDbContext _db,
            IMapper _mapper
          ) 
        { 
            this._db = _db;
            this._mapper = _mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCoursesAsync()
        {
            var query = from course in _db.Courses
                        join subject in _db.Subjects on course.SubjectId equals subject.SubjectId
                        join semester in _db.Semesters on course.SemesterId equals semester.SemesterId
                        join instructor in _db.Users on course.LecturerId equals instructor.Id
                        select new CourseDto
                        {
                            CourseId = course.CourseId,
                            SubjectId =subject.SubjectId,
                            SemesterId =semester.SemesterId,
                            CourseGroup =course.CourseGroup,
                            Background =course.Background,
                            InviteCode =course.InviteCode,
                            EnableInvite = course.EnableInvite,
                            LecturerId = instructor.Id,
                            CreatedAt =course.CreatedAt,
                            UpdatedAt =course.UpdatedAt,
                            DeletedAt =course.DeletedAt,
                            Semester = _mapper.Map<SemesterDto>(semester),
                            Lecturer = _mapper.Map<UserDto>(instructor),
                            Subject = _mapper.Map<SubjectDto>(subject),
                        };
            var courseDtos = await query.ToListAsync();
            return Ok(new ApiResponse<List<CourseDto>>(200,"Thành công",courseDtos));
        }
        [HttpGet("filter")]
        public async Task<IActionResult> GetCoursesPagingAsync(string filter, int pageIndex, int pageSize)
        {
            var query = _db.Courses.AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(e => e.CourseGroup.Contains(filter));
            }
            var finalQuery = from course in query
                        join instructor in _db.Users on course.LecturerId equals instructor.Id into courseInstructors
                        from instructor in courseInstructors.DefaultIfEmpty()

                        join semester in _db.Semesters on course.SemesterId equals semester.SemesterId into courseSemesters
                        from semester in courseSemesters.DefaultIfEmpty()

                        join subject in _db.Subjects on course.SubjectId equals subject.SubjectId into courseSubjects
                        from subject in courseSubjects.DefaultIfEmpty()
                        select new CourseDto
                        {
                            CourseId = course.CourseId,
                            SubjectId = course.SubjectId,
                            SemesterId = course.SemesterId,
                            CourseGroup = course.CourseGroup,
                            Background = course.Background,
                            InviteCode = course.InviteCode,
                            EnableInvite = course.EnableInvite,
                            LecturerId = course.LecturerId,
                            CreatedAt = course.CreatedAt,
                            UpdatedAt = course.UpdatedAt,
                            DeletedAt = course.DeletedAt,
                            Semester = _mapper.Map<SemesterDto>(semester),
                            Lecturer = _mapper.Map<UserDto>(instructor),
                            Subject = _mapper.Map<SubjectDto>(subject),
                        }; 
            var totalRecords = await finalQuery.CountAsync();
            var items = await finalQuery.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            var pagination = new Pagination<CourseDto>
            {
                Items = items,
                TotalRecords = totalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(pagination);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            var query = from course in _db.Courses where course.CourseId == id
                        join instructor in _db.Users on course.LecturerId equals instructor.Id into courseInstructors
                        from instructor in courseInstructors.DefaultIfEmpty()
                        
                        join semester in _db.Semesters on course.SemesterId equals semester.SemesterId into courseSemesters
                        from semester in courseSemesters.DefaultIfEmpty()
                        
                        join subject in _db.Subjects on course.SubjectId equals subject.SubjectId into courseSubjects
                        from subject in courseSubjects.DefaultIfEmpty()
                        select new CourseDto
                        {
                            CourseId = course.CourseId,
                            SubjectId = course.SubjectId,
                            SemesterId = course.SemesterId,
                            CourseGroup = course.CourseGroup,
                            Background = course.Background,
                            InviteCode = course.InviteCode,
                            EnableInvite = course.EnableInvite,
                            LecturerId = course.LecturerId,
                            CreatedAt = course.CreatedAt,
                            UpdatedAt = course.UpdatedAt,
                            DeletedAt = course.DeletedAt,
                            Semester = _mapper.Map<SemesterDto>(semester),
                            Lecturer = _mapper.Map<UserDto>(instructor),
                            Subject = _mapper.Map<SubjectDto>(subject),
                        };
            var returnCourse = await query.FirstOrDefaultAsync();
            if ( await query.CountAsync() == 0)
            {
                return NotFound(new ApiNotFoundResponse<string>($"Không tìm thấy khóa học với id : {id}"));
            }

            var queryStudent = from student in _db.Users
                        join enrollData in _db.EnrolledCourse on student.Id equals enrollData.StudentId
                        where enrollData.CourseId == id
                        select student;
            returnCourse.Students = _mapper.Map<List<UserDto>>( await queryStudent.ToListAsync());
            return Ok(new ApiResponse<CourseDto>(200,"Thành công", returnCourse));
        }
        [HttpGet("{lecturerId}/filter")]
        public async Task<IActionResult> GetByLecturerIdAsync(string lecturerId, int pageIndex, int pageSize)
        {
            var query = from course in _db.Courses
                        where course.LecturerId == lecturerId
                        join instructor in _db.Users on course.LecturerId equals instructor.Id into courseInstructors
                        from instructor in courseInstructors.DefaultIfEmpty()

                        join semester in _db.Semesters on course.SemesterId equals semester.SemesterId into courseSemesters
                        from semester in courseSemesters.DefaultIfEmpty()

                        join subject in _db.Subjects on course.SubjectId equals subject.SubjectId into courseSubjects
                        from subject in courseSubjects.DefaultIfEmpty()
                        select new CourseDto
                        {
                            CourseId = course.CourseId,
                            SubjectId = course.SubjectId,
                            SemesterId = course.SemesterId,
                            CourseGroup = course.CourseGroup,
                            Background = course.Background,
                            InviteCode = course.InviteCode,
                            EnableInvite = course.EnableInvite,
                            LecturerId = course.LecturerId,
                            CreatedAt = course.CreatedAt,
                            UpdatedAt = course.UpdatedAt,
                            DeletedAt = course.DeletedAt,
                            Semester = _mapper.Map<SemesterDto>(semester),
                            Lecturer = _mapper.Map<UserDto>(instructor),
                            Subject = _mapper.Map<SubjectDto>(subject),
                        };
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            var pagination = new Pagination<CourseDto>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(new ApiResponse<Pagination<CourseDto>>(200,"Thành công",pagination));
        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostCourseAsync(CreateCourseRequestDto requestDto)
        {
            var courses = _db.Courses;
            if(await _db.Courses.AnyAsync(c=>c.CourseGroup == requestDto.CourseGroup && c.SemesterId==requestDto.SemesterId && c.SubjectId == requestDto.SemesterId))
            {
                return BadRequest(new ApiBadRequestResponse<string>("Nhóm môn học được mở không được trùng"));
            }    
            var newCourseId = Guid.NewGuid();
            var newCourse = new Course()
            {
                CourseId = newCourseId.ToString(),
                CourseGroup = requestDto.CourseGroup,
                EnableInvite = requestDto.EnableInvite,
                InviteCode = requestDto.InviteCode ?? GenerateRandomNumericString(6),
                LecturerId = requestDto.LecturerId, 
                SemesterId = requestDto.SemesterId,
                SubjectId = requestDto.SubjectId,   
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
            };
            var result = await _db.AddAsync(newCourse);
            await _db.SaveChangesAsync();
            return Ok(new ApiResponse<CourseDto>(200,"Thành công",_mapper.Map<CourseDto>(newCourse)));
        }
        [HttpPut("{courseId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutCourseId(string courseId, [FromBody] CreateCourseRequestDto requestDto)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if(course == null)
            {
                return NotFound(new ApiNotFoundResponse<string>("Không tìm thấy lớp"));
            }
            if (await _db.Courses.AnyAsync(c => c.CourseGroup == requestDto.CourseGroup && c.SemesterId == requestDto.SemesterId && c.SubjectId == requestDto.SemesterId && c.CourseId != course.CourseId))
            {
                return BadRequest(new ApiBadRequestResponse<string>("Nhóm môn học được mở không được trùng"));
            }
            course.CourseGroup = requestDto.CourseGroup;
            course.EnableInvite = requestDto.EnableInvite;
            course.InviteCode = requestDto.InviteCode;
            course.UpdatedAt = DateTime.Now;
            course.LecturerId = requestDto.LecturerId;
            course.SemesterId = requestDto.SemesterId;
            course.SubjectId = requestDto.SubjectId; 

            _db.Courses.Update(course);
            var result = await _db.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(new ApiResponse<CourseDto>(200,"Cập nhật thành công", _mapper.Map<CourseDto>(course)));
            }
            return BadRequest(new ApiBadRequestResponse<string>("Cập nhật lớp học thất bại"));
        }

        [HttpPut("{courseId}/inviteCode")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutCourseInviteCodeAsync(string courseId,[FromBody]string inviteCode)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return NotFound(new ApiNotFoundResponse<string>("Không tìm thấy lớp"));
            }
            if (string.IsNullOrEmpty(inviteCode))
            {
                return BadRequest(new ApiBadRequestResponse<string>("Mã mời không được trùng"));
            }
            course.InviteCode = inviteCode; 
            _db.Courses.Update(course);
            var result = await _db.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(new ApiResponse<CourseDto>(200, "Cập nhật mã mời thành công", _mapper.Map<CourseDto>(course)));
            }
            return BadRequest(new ApiBadRequestResponse<string>("Cập nhật mã mời thất bại"));
        }
        [HttpGet("{courseId}/inviteCode/{inviteCode:length(1,100)}")]
        [Authorize]
        public async Task<IActionResult> GetApplyCodeAsync(string courseId, string inviteCode)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return NotFound(new ApiNotFoundResponse<string>("Không tìm thấy lớp học"));
            }
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (course.InviteCode != inviteCode)
            {
                return BadRequest(new ApiBadRequestResponse<string>("Mã lớp học không chính xác"));
            }
            if (!await _db.EnrolledCourse.AnyAsync(c=>c.CourseId == courseId && c.StudentId == userId))
            {
                await _db.EnrolledCourse.AddAsync(new EnrolledCourse()
                {
                    CourseId = courseId,
                    StudentId = userId,
                    CreatedAt = DateTime.Now,
                });
                var result = await _db.SaveChangesAsync();
                if (result > 0)
                {
                    return RedirectToAction("GetById", new { id = courseId });
                }
                else
                {
                    return BadRequest(new ApiBadRequestResponse<string>("Không thể tham gia lớp học"));
                }
            }
            else
            {
                return RedirectToAction("GetById", new { id = courseId });
            }

        }
        [HttpDelete("{courseId}")]
        public async Task<IActionResult> DeleteCourseAsync(string courseId)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return NotFound(new ApiNotFoundResponse<string>($"Không thể tìm thấy lớp học với id {courseId}"));
            }
            _db.Courses.Remove(course);
            var result = await _db.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(new ApiResponse<CourseDto>(200,"Xóa thành công",_mapper.Map<CourseDto>(course)));
            }
            return BadRequest(new ApiBadRequestResponse<string>("Xóa thông tin lớp học thất bại"));
        }
        [HttpGet("{courseId}/groups")]
        public async Task<IActionResult> GetCourseGroupsAsync(string courseId)
        {
            var groups = from course in _db.Courses where course.CourseId == courseId
                         join g in _db.Groups on course.CourseId equals g.CourseId into gCourse
                         from g in gCourse.DefaultIfEmpty()
                         select new GroupDto
                         {
                             GroupId = g.GroupId,
                             GroupName = g.GroupName,
                             ProjectId = g.ProjectId,
                             CourseId= courseId,
                             NumberOfMembers = g.NumberOfMembers,
                             CreatedAt = g.CreatedAt,
                             UpdatedAt = g.UpdatedAt,
                             DeletedAt = g.DeletedAt,
                             CourseGroup = course.CourseGroup,
                         };
            if (groups == null)
            {
                return NotFound(new ApiNotFoundResponse<string>($"Không thể tìm thấy lớp học với id : {courseId}"));
            }
            return Ok(await groups.ToListAsync());

        }

        [HttpGet("{courseId}/announcements/filter")]
        public async Task<IActionResult> GetAnnouncementsInCourseAsync(string courseId,string filter, int pageIndex, int pageSize)
        {
            var query = from announcement in _db.Announcements where announcement.CourseId == courseId
                        join user in _db.Users on announcement.UserId equals user.Id into announcementUsers
                        from user in announcementUsers.DefaultIfEmpty()
                        select new AnnouncementDto
                        {
                            AnnouncementId = announcement.AnnouncementId,
                            UserId = announcement.UserId,
                            CourseId = announcement.CourseId,
                            Content = announcement.Content,
                            AttachedLinks = announcement.AttachedLinks,
                            CreatedAt = announcement.CreatedAt,
                            UpdatedAt = announcement.UpdatedAt,
                            DeletedAt = announcement.DeletedAt,
                            CreateUser = _mapper.Map<UserDto>(user)
                        };
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            var data = items.Select(e => _mapper.Map<AnnouncementDto>(e)).ToList();

            var pagination = new Pagination<AnnouncementDto>
            {
                Items = data,
                TotalRecords = totalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(new ApiResponse<Pagination<AnnouncementDto>>(200, "Thành công", pagination));
        }
        //[HttpGet("{courseId}/students")]
        //public async Task<IActionResult> GetStudentsInCourseAsync(string courseId,string filter, int pageIndex, int pageSize)
        //{
        //    var query = from student in _db.Users
        //                join enrollData in _db.EnrolledCourse on student.Id equals enrollData.StudentId
        //                where enrollData.CourseId == courseId
        //                select student;
        //    var totalRecords = await query.CountAsync();
        //    var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

        //    var pagination = new Pagination<UserDto>
        //    {
        //        Items = _mapper.Map<List<UserDto>>(items),
        //        TotalRecords = totalRecords,
        //        PageIndex = pageIndex,
        //        PageSize = pageSize
        //    };
        //    return Ok(new ApiResponse<Pagination<UserDto>>(200, "Thành công", pagination));
        //}
        [HttpGet("{courseId}/projects")]
        public async Task<IActionResult> GetProjectsInCourseAsync(string courseId)
        {
            var query = from project in _db.Projects
                        where project.CourseId == courseId
                        join user in _db.Users on project.CreateUserId equals user.Id into projectUsers
                        from user in projectUsers.DefaultIfEmpty()
                        select new ProjectDto
                        {
                            ProjectId = project.ProjectId,
                            CourseId =project.CourseId,
                            CreateUserId = project.CreateUserId,
                            Description = project.Description,
                            IsApproved = project.IsApproved,
                            Title = project.Title,
                            CreatedAt = project.CreatedAt,
                            UpdatedAt = project.UpdatedAt,
                            DeletedAt =project.DeletedAt,
                            CreateUser = _mapper.Map<UserDto>(user)
                        };
            return Ok(new ApiResponse<List<ProjectDto>>(200, "Thành công", await query.ToListAsync()));
        }
        private string GenerateRandomNumericString(int length)
        {
            Random random = new Random();
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = (char)('0' + random.Next(0, 10));
            }

            return new string(result);
        }

    }
}
