using AutoMapper;
using KLTN.Application.DTOs.Assignments;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using File = KLTN.Domain.Entities.File;

namespace KLTN.Application.Services
{
    public class AssignmentService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly CourseService courseService;   
        public AssignmentService(IUnitOfWork unitOfWork, 
            CourseService courseService, 
            UserManager<User> userManager, 
            IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.courseService = courseService;
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
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefault(c => c.AssignmentId == assignmentId);
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
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefault(c => c.AssignmentId == assignmentId);
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
            assignment.AttachedLinks = requestDto.AttachedLinks;
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
                AttachedLinks = requestDto.AttachedLinks,
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
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefault(c => c.AssignmentId == assignmentId);
            if (assignment == null)
            {
                return null;
            }
            var assignmentDto = mapper.Map<AssignmentDto>(assignment);
            assignmentDto.Course = await courseService.GetCourseDtoByIdAsync(assignmentDto.CourseId, false, false);
            return assignmentDto;
        }
        public async Task<List<AssignmentDto>> GetAssignmentDtosInCourseAsync(string courseId)
        {
            var assignments = unitOfWork.AssignmentRepository.GetAll(c => c.CourseId == courseId);
            var assignmentDtos = mapper.Map<List<AssignmentDto>>(assignments);
            for (int i = 0; i < assignmentDtos.Count; i++)
            {
                assignmentDtos[i] = await GetAssignmentDtoByIdAsync(assignmentDtos[i].AssignmentId);
            }
            return assignmentDtos;
        }
        #endregion

    }
}
