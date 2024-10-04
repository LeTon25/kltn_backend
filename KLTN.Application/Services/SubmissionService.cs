using AutoMapper;
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
    public class SubmissionService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly CourseService courseService;  
        private readonly UserManager<User> userManager;
        private readonly CommentService commentService;
        public SubmissionService(IUnitOfWork unitOfWork, 
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
        public async Task<ApiResponse<object>> GetSubmissionByIdAsync(string submissionId,string currentUserId)
        {
            var data = await unitOfWork.SubmissionRepository.GetFirstOrDefaultAsync(c => c.SubmissionId.Equals(submissionId), false, c => c.CreateUser, c => c.Assignment, c => c.Assignment.Course);
            if (data == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy bài nộp");
            }
            if(currentUserId != data.UserId && currentUserId != data.Assignment.Course.LecturerId)
            {
                return new ApiBadRequestResponse<object>("Bạn không có quyền xem bài nộp này");
            }
            var dto = mapper.Map<SubmissionDto>(data);
            return new ApiResponse<object>(200, "Thành công", dto);
        }
        public async Task<ApiResponse<object>> DeleteSubmissionAsync(string userId,string submissionId)
        {
            var submission = await unitOfWork.SubmissionRepository.GetFirstOrDefaultAsync(c => c.SubmissionId == submissionId,false,c=>c.Assignment,c => c.Assignment.Course);
            if (submission == null)
            {
                return new ApiNotFoundResponse<object>("Không thể tìm thấy bài nộp với id");
            }
            if(userId != submission.Assignment.Course.LecturerId)
            {
                return new ApiBadRequestResponse<object>("Không có quyền xóa bài nộp");
            }    
            unitOfWork.SubmissionRepository.Delete(submission);
            var result = await unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Thành công", mapper.Map<SubmissionDto>(submission));
            }
            return new ApiBadRequestResponse<object>("Xóa thông tin bài nộp thất bại");
        }
        public async Task<ApiResponse<object>> UpdateSubmissionAsync(string currentUserId,string submissionId, CreateSubmissionDto requestDto)
        {
            var submission = await unitOfWork.SubmissionRepository.GetFirstOrDefaultAsync(c => c.SubmissionId == submissionId,false,c=>c.CreateUser,c => c.Assignment);
            if (submission == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy bài nộp");
            }
            if (currentUserId != submission.UserId)
            {
                return new ApiBadRequestResponse<object>("Bạn không có quyền chỉnh sửa bài tập này");
            }
            var dateTimeNow = DateTime.Now;
            if (submission.Assignment.DueDate != null && dateTimeNow > submission.Assignment.DueDate )
            {
                return new ApiBadRequestResponse<object>("Hết hạn để nộp hay chỉnh sửa");
            }
            if (!requestDto.Attachments.Any() && !requestDto.AttachedLinks.Any())
            {
                return new ApiBadRequestResponse<object>("Vui lòng đính kèm ít nhất một file");
            }
            submission.AttachedLinks = mapper.Map<List<MetaLinkData>>(requestDto.AttachedLinks);
            submission.UpdatedAt = DateTime.Now;
            submission.Attachments = mapper.Map<List<KLTN.Domain.Entities.File>>(requestDto.Attachments);
            submission.Description = requestDto.Description;
            unitOfWork.SubmissionRepository.Update(submission);
            var result = await unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                var responseDto = mapper.Map<SubmissionDto>(submission);
                return new ApiResponse<object>(200, "Cập nhập thành công", responseDto);
            }
            return new ApiBadRequestResponse<object>("Cập nhật bài nộp thất bại");
        }
        public async Task<ApiResponse<object>> CreateSubmissionAsync(string currentUserId,CreateSubmissionDto requestDto,string assignmentId)
        {
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefaultAsync(c => c.AssignmentId.Equals(assignmentId), false, c => c.Course, c => c.Course.EnrolledCourses);
            if (assignment == null) 
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy bài tập để nộp");
            }
            var dateTimeNow = DateTime.Now;
            if (assignment.DueDate != null && dateTimeNow > assignment.DueDate)
            {
                return new ApiBadRequestResponse<object>("Hết hạn để nộp hay chỉnh sửa");
            }
            if (currentUserId == assignment?.Course?.LecturerId)
            {
                return new ApiBadRequestResponse<object>("Giáo viên của lớp không thể nộp bài");
            }
            if ( assignment.Course.EnrolledCourses == null || !assignment.Course.EnrolledCourses.Any(c => c.StudentId.Equals(currentUserId)))
            {
                return new ApiBadRequestResponse<object>("Bạn không có trong lớp học");
            }
            if (!requestDto.Attachments.Any() && !requestDto.AttachedLinks.Any())
            {
                return new ApiBadRequestResponse<object>("Vui lòng đính kèm ít nhất một file");
            }
            var newSubmissionId = Guid.NewGuid();
            var newSubmission = new Submission()
            {
                SubmissionId = newSubmissionId.ToString(),
                Description = requestDto.Description,
                AttachedLinks = mapper.Map<List<MetaLinkData>>(requestDto.AttachedLinks),
                Attachments = mapper.Map<List<File>>(requestDto.Attachments),
                AssignmentId = assignmentId,
                UserId = currentUserId,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
            };
            await unitOfWork.SubmissionRepository.AddAsync(newSubmission);
            await unitOfWork.SaveChangesAsync();
            var responseDto = mapper.Map<SubmissionDto>(newSubmission);
            var user = await userManager.FindByIdAsync(currentUserId);
            responseDto.CreateUser = mapper.Map<UserDto>(user);
            return new ApiResponse<object>(200, "Tạo thành công", mapper.Map<SubmissionDto>(newSubmission));
        }
        #endregion
    }
}
