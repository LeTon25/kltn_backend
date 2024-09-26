using AutoMapper;
using KLTN.Application.DTOs.Assignments;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Enums;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using File = KLTN.Domain.Entities.File;

namespace KLTN.Application.Services
{
    public class AssignmentService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly CourseService courseService;  
        private readonly UserManager<User> userManager;
        private readonly CommentService commentService;
        public AssignmentService(IUnitOfWork unitOfWork, 
            CourseService courseService, 
            UserManager<User> userManager, 
            CommentService commentService,
            IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager; 
            this.courseService = courseService;
            this.commentService = commentService;   
        }
        #region for_controller
        public async Task<ApiResponse<object>> GetAssignmentByIdAsync(string assignmentId)
        {
            var data = await GetAssignmentDtoByIdAsync(assignmentId);
            if (data == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy bài tập");
            }
            return new ApiResponse<object>(200, "Thành công", data);
        }
        public async Task<ApiResponse<object>> DeleteAssignmentAsync(string userId,string assignmentId)
        {
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefaultAsync(c => c.AssignmentId == assignmentId);
            if (assignment == null)
            {
                return new ApiNotFoundResponse<object>("Không thể tìm thấy bài tập với id");
            }
            if (!await courseService.CheckIsTeacherAsync(userId, assignment.CourseId))
            {
                return new ApiResponse<object>(403, "Bạn không có quyền xóa bài tập này", null);
            }
            unitOfWork.AssignmentRepository.Delete(assignment);
            var result = await unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Thành công", mapper.Map<AssignmentDto>(assignment));
            }
            return new ApiBadRequestResponse<object>("Xóa thông tin bài tập thất bại");
        }
        public async Task<ApiResponse<object>> UpdateAssignmentAsync(string userId,string assignmentId, UpSertAssignmentRequestDto requestDto)
        {
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefaultAsync(c => c.AssignmentId == assignmentId);
            if (assignment == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy bài tập");
            }
            if (!await courseService.CheckIsTeacherAsync(userId, assignment.CourseId))
            {
                return new ApiResponse<object>(403, "Bạn không có quyền chỉnh sửa bài tập này", null);
            }

            assignment.Title = requestDto.Title;
            assignment.Content = requestDto.Content;
            assignment.CourseId = requestDto.CourseId;
            assignment.DueDate = requestDto.DueDate;
            assignment.StudentAssigned = requestDto.StudentAssigned;
            assignment.AttachedLinks = mapper.Map<List<MetaLinkData>>(requestDto.AttachedLinks);
            assignment.UpdatedAt = DateTime.Now;
            assignment.Attachments = mapper.Map<List<KLTN.Domain.Entities.File>>(requestDto.Attachments);

            unitOfWork.AssignmentRepository.Update(assignment);
            var result = await unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                var responseDto = await GetAssignmentDtoByIdAsync(assignmentId);
                return new ApiResponse<object>(200, "Cập nhập thành công", responseDto);
            }
            return new ApiBadRequestResponse<object>("Cập nhật bài tập thất bại");
        }
        public async Task<ApiResponse<object>> CreateAssignmentAsync(string userId,UpSertAssignmentRequestDto requestDto)
        {
            var newAssignmentId = Guid.NewGuid();
            var newAssignment = new Assignment()
            {
                AssignmentId = newAssignmentId.ToString(),
                Content = requestDto.Content,
                CourseId = requestDto.CourseId,
                Title = requestDto.Title,   
                DueDate = requestDto.DueDate,
                AttachedLinks = mapper.Map<List<MetaLinkData>>(requestDto.AttachedLinks),
                Attachments = mapper.Map<List<File>>(requestDto.Attachments),
                StudentAssigned = requestDto.StudentAssigned,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
            };
            if (!await courseService.CheckIsTeacherAsync(userId, newAssignment.CourseId))
            {
                return new ApiResponse<object>(403, "Bạn không có quyền tạo bài tập", null);
            }
            await unitOfWork.AssignmentRepository.AddAsync(newAssignment);
            await unitOfWork.SaveChangesAsync();
            var responseDto = mapper.Map<AssignmentDto>(newAssignment);
            return new ApiResponse<object>(200, "Cập nhập thành công", mapper.Map<AssignmentDto>(newAssignment));
        }
        #endregion

        #region for_service
        public async Task<AssignmentDto> GetAssignmentDtoByIdAsync(string assignmentId)
        {
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefaultAsync(c => c.AssignmentId == assignmentId);
            if (assignment == null)
            {
                return null;
            }
            var assignmentDto = mapper.Map<AssignmentDto>(assignment);
            assignmentDto.Course = await courseService.GetCourseDtoByIdAsync(assignmentDto.CourseId, false, false);

            var userEntity = await userManager.Users.FirstOrDefaultAsync(c => c.Id.Equals(assignmentDto.Course.LecturerId));
            assignmentDto.CreateUser = mapper.Map<UserDto>(userEntity);

            assignmentDto.Comments = await commentService.GetCommentDtosFromPostAsync(assignmentId, CommentableType.Assignment);
            return assignmentDto;
        }
        #endregion

    }
}
