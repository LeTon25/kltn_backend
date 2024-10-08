using AutoMapper;
using KLTN.Application.DTOs.Assignments;
using KLTN.Application.DTOs.Submissions;
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
        public async Task<ApiResponse<object>> GetAssignmentByIdAsync(string assignmentId,string currentUserId)
        {
            var data = await GetAssignmentDtoByIdAsync(assignmentId,currentUserId);
            if (data == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy bài tập");
            }
            return new ApiResponse<object>(200, "Thành công", data);
        }
        public async Task<ApiResponse<object>> DeleteAssignmentAsync(string userId,string assignmentId)
        {
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefaultAsync(c => c.AssignmentId == assignmentId,false,c=>c.Course);
            if (assignment == null)
            {
                return new ApiNotFoundResponse<object>("Không thể tìm thấy bài tập với id");
            }
            if (assignment.Course.LecturerId != userId)
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
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefaultAsync(c => c.AssignmentId == assignmentId,false,c=>c.Course);
            if (assignment == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy bài tập");
            }
            if (assignment.Course.LecturerId != userId)
            {
                return new ApiResponse<object>(403, "Bạn không có quyền chỉnh sửa bài tập này", null);
            }
            if(requestDto.ScoreStructureId != null)
            {
                var scoreStructure = await unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c => c.Id.Equals(requestDto.ScoreStructureId), false, c => c.Children);
                if (scoreStructure == null || scoreStructure.Children.Count > 0)
                {
                    return new ApiBadRequestResponse<object>("Cột điểm không hợp lệ");
                }
                if (await unitOfWork.AssignmentRepository.AnyAsync(c => c.CourseId.Equals(requestDto.CourseId) && c.ScoreStructureId == requestDto.ScoreStructureId && c.AssignmentId != assignmentId))
                {
                    return new ApiBadRequestResponse<object>("Cột điểm đã được chấm bởi bài tập khác");
                }
            }    

            assignment.Title = requestDto.Title;
            assignment.Content = requestDto.Content;
            assignment.CourseId = requestDto.CourseId;
            assignment.DueDate = requestDto.DueDate;
            assignment.AttachedLinks = mapper.Map<List<MetaLinkData>>(requestDto.AttachedLinks);
            assignment.UpdatedAt = DateTime.Now;
            assignment.Attachments = mapper.Map<List<KLTN.Domain.Entities.File>>(requestDto.Attachments);
            assignment.IsGroupAssigned =requestDto.IsGroupAssigned;
            assignment.ScoreStructureId = requestDto.ScoreStructureId;
            unitOfWork.AssignmentRepository.Update(assignment);
            var result = await unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                var responseDto = await GetAssignmentDtoByIdAsync(assignmentId, userId);
                return new ApiResponse<object>(200, "Cập nhập thành công", responseDto);
            }
            return new ApiBadRequestResponse<object>("Cập nhật bài tập thất bại");
        }
        public async Task<ApiResponse<object>> CreateAssignmentAsync(string userId,UpSertAssignmentRequestDto requestDto)
        {
            if (requestDto.ScoreStructureId != null)
            {
                var scoreStructure = await unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c => c.Id.Equals(requestDto.ScoreStructureId), false, c => c.Children);

                if(scoreStructure == null || scoreStructure.Children.Count > 0)
            {
                return new ApiBadRequestResponse<object>("Cột điểm không hợp lệ");
            }
                if (await unitOfWork.AssignmentRepository.AnyAsync(c => c.CourseId.Equals(requestDto.CourseId) && c.ScoreStructureId == requestDto.ScoreStructureId))
                {
                    return new ApiBadRequestResponse<object>("Cột điểm đã được chấm bởi bài tập khác");
                }

            }
            var newAssignmentId = Guid.NewGuid();
            var newAssignment = new Assignment()
            {
                AssignmentId = newAssignmentId.ToString(),
                Content = requestDto.Content,
                CourseId = requestDto.CourseId,
                Title = requestDto.Title,
                ScoreStructureId = requestDto.ScoreStructureId,
                DueDate = requestDto.DueDate,
                AttachedLinks = mapper.Map<List<MetaLinkData>>(requestDto.AttachedLinks),
                Attachments = mapper.Map<List<File>>(requestDto.Attachments),
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                IsGroupAssigned = requestDto.IsGroupAssigned,
            };
            if (!await courseService.CheckIsTeacherAsync(userId, newAssignment.CourseId))
            {
                return new ApiResponse<object>(403, "Bạn không có quyền tạo bài tập", null);
            }
            await unitOfWork.AssignmentRepository.AddAsync(newAssignment);
            await unitOfWork.SaveChangesAsync();
            var responseDto = await GetAssignmentDtoByIdAsync(newAssignment.AssignmentId, userId);

            return new ApiResponse<object>(200, "Tạo thành công", mapper.Map<AssignmentDto>(newAssignment));
        }
        public async Task<ApiResponse<object>> GetSubmissionsInAssignmentsAsync(string userId,string assignmentId)
        {
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefaultAsync(c => c.AssignmentId.Equals(assignmentId), false, c => c.Course);
            if(assignment.Course.LecturerId != userId)
            {
                return new ApiBadRequestResponse<object>("Bạn không có quyền lấy danh sách bài nộp");
            }    
            var submissions = await unitOfWork.SubmissionRepository.FindByCondition(c=>c.AssignmentId.Equals(assignmentId),false,c=>c.CreateUser,c => c.Assignment,c => c.Scores).ToListAsync();
            var submissionDtos = mapper.Map<List<SubmissionDto>>(submissions);  
            
            return new ApiResponse<object>(200,"Lấy dữ liệu thành công",submissionDtos);
        }
        #endregion

        #region for_service
        public async Task<AssignmentDto> GetAssignmentDtoByIdAsync(string assignmentId,string currentUserId)
        {
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefaultAsync(c => c.AssignmentId == assignmentId,false,c=>c.ScoreStructure);
            if (assignment == null)
            {
                return null;
            }
            var assignmentDto = mapper.Map<AssignmentDto>(assignment);
            assignmentDto.Course = await courseService.GetCourseDtoByIdAsync(assignmentDto.CourseId, false, false,false,false);

            var userEntity = await userManager.Users.FirstOrDefaultAsync(c => c.Id.Equals(assignmentDto.Course.LecturerId));
            assignmentDto.CreateUser = mapper.Map<UserDto>(userEntity);

            assignmentDto.Comments = await commentService.GetCommentDtosFromPostAsync(assignmentId, CommentableType.Assignment);
            if(currentUserId != assignmentDto.Course.LecturerId)
            {
                var submission = await unitOfWork.SubmissionRepository.GetFirstOrDefaultAsync(c => c.AssignmentId.Equals(assignment.AssignmentId),false,c=>c.CreateUser);
                assignmentDto.Submission = mapper.Map<SubmissionDto>(submission);
            }
    
            return assignmentDto;
        }
        #endregion

    }
}
