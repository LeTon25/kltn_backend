using AutoMapper;
using KLTN.Application.DTOs.Projects;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace KLTN.Application.Services
{
    public class ProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<User> userManager;
        public ProjectService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
        }
        public async Task<ApiResponse<List<ProjectDto>>> GetAllProjectsAsync()
        {
            var projects = await _unitOfWork.ProjectRepository.FindByCondition(c=>true,false,c=>c.User!).ToListAsync();
            var dto = mapper.Map<List<ProjectDto>>(projects);

            return new ApiResponse<List<ProjectDto>>(200, "Lấy dữ liệu thành công", dto);
        }
        public async Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(string projectId,string currentUserId)
        {
            var project = await _unitOfWork.ProjectRepository.GetFirstOrDefaultAsync(
                c => c.ProjectId.Equals(projectId),
                false,
                c=>c.Course!,
                c =>c.Course.EnrolledCourses
                );
   
            if(project == null)
            {
                return new ApiNotFoundResponse<ProjectDto>("Không tìm thấy đề tài");
            }
            if (currentUserId != project.Course.LecturerId &&
                !project.Course.EnrolledCourses.Any(e => e.StudentId.Equals(currentUserId)))
            {
                return new ApiResponse<ProjectDto>(200,"Không được quyền xem đề tài");

            }
            var projectDto = mapper.Map<ProjectDto>(project);
            var createUser = _unitOfWork.UserRepository.GetFirstOrDefaultAsync(c => c.Id.Equals(project.CreateUserId),false);

            projectDto.CreateUser = mapper.Map<UserDto>(createUser);

            return new ApiResponse<ProjectDto>(200, "Lấy dữ liệu thành công", projectDto);
        }
        public async Task<ApiResponse<ProjectDto>> CreateProjectAsync(string currentUserId , CreateProjectRequestDto dto)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId.Equals(dto.CourseId), false,c=>c.Setting!,c => c.EnrolledCourses!);
            if (course == null) 
            {
                return new ApiNotFoundResponse<ProjectDto>("Không tìm thấy lớp học");
            }
            if(await _unitOfWork.ProjectRepository.AnyAsync(c => c.Title.Equals(dto.Title) && c.CourseId == dto.CourseId))
            {
                return new ApiBadRequestResponse<ProjectDto>("Tên đề tài không được trùng");
            }
            var newProject = new Project()
            {
                ProjectId = Guid.NewGuid().ToString(),
                Title = dto.Title,
                CourseId = dto.CourseId,
                Description = dto.Description,
                CreatedAt = DateTime.Now,
                IsApproved = currentUserId != course.LecturerId ? false : true,
                CreateUserId = dto.CreateUserId,
                UpdatedAt = null,
                DeletedAt = null,
            };
            if (currentUserId != course.LecturerId)
            {
                if (!course.Setting!.AllowStudentCreateProject)
                {
                    return new ApiBadRequestResponse<ProjectDto>("Giảng viên không cho phép SV tạo đề tài");
                }

                if (course.EnrolledCourses != null && !course.EnrolledCourses.Any(c => c.StudentId == currentUserId && c.CourseId == course.CourseId)) 
                { 
                    return new ApiBadRequestResponse<ProjectDto>("Không thể tạo đề tài khi chưa tham gia lớp");
                }
            }
            await _unitOfWork.ProjectRepository.AddAsync(newProject);
            await _unitOfWork.SaveChangesAsync();

            var projectDto = mapper.Map<ProjectDto>(newProject);

            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(c=>c.Id.Equals(currentUserId));

            projectDto.CreateUser = mapper.Map<UserDto>(user);

            return new ApiResponse<ProjectDto>(200,"Thành công",projectDto);
        }
        public async Task<ApiResponse<ProjectDto>> DeleteProjectAsync(string currentUserId, string projectId)
        {
            var project = await _unitOfWork.ProjectRepository.GetFirstOrDefaultAsync(c => c.ProjectId.Equals(projectId), false, c => c.Course);
            if(project == null)
            {
                return new ApiResponse<ProjectDto>(404,"Không tìm thấy đề tài");
            }
            if (!(project.Course.LecturerId == currentUserId || project.CreateUserId == currentUserId))
            {
                return new ApiBadRequestResponse<ProjectDto>("Bạn không có quyền xóa đề tài này");
            }
            _unitOfWork.ProjectRepository.Delete(project);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<ProjectDto>(200, "Thành công", mapper.Map<ProjectDto>(project));
            }
            return new ApiBadRequestResponse<ProjectDto>("Xóa thông tin đề tài thất bại");
        }
        public async Task<ApiResponse<ProjectDto>> UpdateProjectAsync(string currentUserId,string projectId,CreateProjectRequestDto dto)
        {
            var project = await _unitOfWork.ProjectRepository.GetFirstOrDefaultAsync(c => c.ProjectId == projectId,false,c=>c.Course!,c => c.User!);
            if (project == null)
            {
                return new ApiNotFoundResponse<ProjectDto>($"Không tìm thấy đề tài với id : {projectId}");
            }
            if (await _unitOfWork.ProjectRepository.AnyAsync(e => e.Title == dto.Title && e.CourseId == dto.CourseId && e.ProjectId != projectId))
            {
                return new ApiBadRequestResponse<ProjectDto>("Tên đề tài không được trùng");
            }
            if (project.IsApproved != dto.IsApproved && currentUserId != project.Course!.LecturerId)
            {
                new ApiBadRequestResponse<string>("Chỉ có giáo viên mới có thể xét duyệt đề tài");
            }
            project.Title = dto.Title;
            project.Description = dto.Description;
            project.UpdatedAt = DateTime.Now;
            project.IsApproved = dto.IsApproved;

            _unitOfWork.ProjectRepository.Update(project);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<ProjectDto>(200, "Thành công", mapper.Map<ProjectDto>(project));
            }
            return new ApiBadRequestResponse<ProjectDto>("Cập nhật đề tài thất bại");
        }
        public async Task<ProjectDto> GetProjectDtoAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            var project = await _unitOfWork.ProjectRepository.GetFirstOrDefaultAsync(c => c.ProjectId == id,false,c=>c.User!);
            if (project == null)
            {
                return null;
            }
            var projectDto = mapper.Map<ProjectDto>(project);
            projectDto.CreateUser = mapper.Map<UserDto>(project.User);

            return projectDto;
        }
    }
}
