using AutoMapper;
using KLTN.Application.DTOs.Projects;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class ProjectsController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public ProjectsController(ApplicationDbContext db, IMapper mapper)
        {
            this._db = db;
            this._mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetProjectsAsync()
        {
            var query = from project in _db.Projects
                        join user in _db.Users on project.CreateUserId equals user.Id into projectUser
                        from user in projectUser.DefaultIfEmpty()
                        select new ProjectDto
                        {
                            ProjectId = project.ProjectId,
                            CourseId = project.CourseId,
                            CreateUserId =project.CreateUserId,
                            Description =project.Description,
                            IsApproved = project.IsApproved,
                            Title = project.Title,
                            CreatedAt = project.CreatedAt,
                            UpdatedAt =project.UpdatedAt,
                            DeletedAt =project.DeletedAt,
                            CreateUser = _mapper.Map<UserDto>(user),

                        };
            var projectDtos = await query.ToListAsync();
            return Ok(new ApiResponse<List<ProjectDto>>(200,"Thành công",projectDtos));
        }
        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetByIdAsync(string projectId)
        {
            var finalQuery = from project in _db.Projects where project.ProjectId == projectId

                             join user in _db.Users on project.CreateUserId equals user.Id into projectUser
                             from user in projectUser.DefaultIfEmpty()
                             select new ProjectDto
                             {
                                 ProjectId = project.ProjectId,
                                 CourseId = project.CourseId,
                                 CreateUserId = project.CreateUserId,
                                 Description = project.Description,
                                 IsApproved = project.IsApproved,
                                 Title = project.Title,
                                 CreatedAt = project.CreatedAt,
                                 UpdatedAt = project.UpdatedAt,
                                 DeletedAt = project.DeletedAt,
                                 CreateUser = _mapper.Map<UserDto>(user),
                             };
            if(await finalQuery.CountAsync() == 0)
            {
                return NotFound(new ApiNotFoundResponse<string>("Không tìm thấy đề tài cần tìm"));
            }    
            return Ok(new ApiResponse<ProjectDto>(200, "Thành công", _mapper.Map<ProjectDto>(await finalQuery.FirstOrDefaultAsync())));

        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostProjectAsync(CreateProjectRequestDto requestDto)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == requestDto.CourseId);
   
            if (!(await _db.EnrolledCourse.AnyAsync(c => c.StudentId == currentUserId && c.CourseId ==course.CourseId)) && currentUserId != course.LecturerId)
            {
                return BadRequest(new ApiBadRequestResponse<string>("Không có quyền tạo/sửa đề tài trong lớp này"));
            }
            var projects = _db.Projects;
            if (await projects.AnyAsync(c => c.Title.Equals(requestDto.Title) && c.CourseId == requestDto.CourseId))
            {
                return BadRequest(new ApiBadRequestResponse<string>("Tên đề tài không được trùng"));
            }
            var newProjectId = Guid.NewGuid();
            var newProject = new Project()
            {
                ProjectId = newProjectId.ToString(),
                Title = requestDto.Title,
                CourseId = requestDto.CourseId,
                Description = requestDto.Description,
                CreatedAt = DateTime.Now,
                IsApproved = requestDto.IsApproved,
                CreateUserId = requestDto.CreateUserId,
                UpdatedAt = null,
                DeletedAt = null,
            };
            var result = await _db.AddAsync(newProject);
            await _db.SaveChangesAsync();
            return Ok(new ApiResponse<ProjectDto>(200, "Thành công", _mapper.Map<ProjectDto>(newProject)));
        }
        [HttpPatch("{projectId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutProjectId(string projectId, [FromBody] CreateProjectRequestDto requestDto)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var project = await _db.Projects.FirstOrDefaultAsync(c => c.ProjectId == projectId);
            if (project == null)
            {
                return NotFound(new ApiNotFoundResponse<string>($"Không tìm thấy đề tài với id : {projectId}"));
            }
            if (await _db.Projects.AnyAsync(e => e.Title == requestDto.Title && e.CourseId == requestDto.CourseId && e.ProjectId != projectId))
            {
                return BadRequest(new ApiBadRequestResponse<string>("Tên đề tài không được trùng"));
            }
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == project.CourseId);
            if (project.IsApproved != requestDto.IsApproved && currentUserId != course.LecturerId)
            {
                return BadRequest(new ApiBadRequestResponse<string>("Chỉ có giáo viên mới có thể xét duyệt đề tài"));
            }
            project.Title = requestDto.Title;
            project.Description = requestDto.Description;
            project.UpdatedAt=DateTime.Now;
            project.IsApproved = requestDto.IsApproved;
            
            _db.Projects.Update(project);

            var result = await _db.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(new ApiResponse<ProjectDto>(200,"Thành công",_mapper.Map<ProjectDto>(project)));
            }
            return BadRequest(new ApiBadRequestResponse<string>("Cập nhật đề tài thất bại"));
        }

        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteProjectAsync(string projectId)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var project = await _db.Projects.FirstOrDefaultAsync(c => c.ProjectId == projectId);
            if (project == null)
            {
                return NotFound(new ApiNotFoundResponse<string>("Không thể tìm thấy đề tài với id"));
            }
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == project.CourseId);
            if(!(course.LecturerId == currentUserId  || project.CreateUserId == currentUserId))
            {
                return BadRequest(new ApiBadRequestResponse<object>("Bạn không có quyền xóa đề tài này"));
            }    
            _db.Projects.Remove(project);
            var result = await _db.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(new ApiResponse<ProjectDto>(200, "Thành công", _mapper.Map<ProjectDto>(project)));
            }
            return BadRequest(new ApiBadRequestResponse<string>("Xóa thông tin đề tài thất bại"));
        }
    }
}
