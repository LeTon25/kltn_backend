using AutoMapper;
using KLTN.Application.DTOs.Comments;
using KLTN.Application.DTOs.ReportComments;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Services
{
    public class ReportCommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<User> userManager;
        public ReportCommentService(IUnitOfWork unitOfWork, IMapper mapper,UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
        }
        public async Task<ApiResponse<object>> CreateReportCommentsAsync(string announcementId, CreateReportCommentRequestDto request,string userId)
        {

            var comment = new ReportComment()
            {
                Content = request.Content,
                ReportId = announcementId,
                UserId = userId,
                CreatedAt = DateTime.Now,
                ReportCommentId = Guid.NewGuid().ToString(),
            };
            await _unitOfWork.ReportCommentRepository.AddAsync(comment);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                var commentDto = mapper.Map<ReportCommentDto>(comment);
                var user = await userManager.FindByIdAsync(userId);
                commentDto.User = mapper.Map<UserDto>(user);
                return new ApiResponse<object>(200, "Thành công",commentDto);
            }
            else
            {
                return new ApiBadRequestResponse<object>("Bình luận thất bại");
            }
        }
        public async Task<ApiResponse<object>> UpdateReportCommentAsync(string commentId, CreateReportCommentRequestDto request,string userId)
        {
            var comment = await _unitOfWork.ReportCommentRepository.GetFirstOrDefaultAsync(c=>c.ReportCommentId == commentId);
            if (comment == null)
                return new ApiBadRequestResponse<object>($"Không thể tìm thấy bình luận với id : {commentId}");
            if (comment.UserId != userId)
                return new ApiResponse<object>(403,"Không thể cập nhật comment");

            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.Now;
            _unitOfWork.ReportCommentRepository.Update(comment);

            var result = await _unitOfWork.SaveChangesAsync();

            if (result > 0)
            {
                var commentDto = mapper.Map<ReportCommentDto>(comment);
                var user = await userManager.FindByIdAsync(userId);
                commentDto.User = mapper.Map<UserDto>(user);
                return new ApiResponse<object>(200, "Cập nhật thành công",commentDto);
            }
            return new ApiBadRequestResponse<object>($"Update comment failed");
        }
        public async Task<ApiResponse<object>> DeleteReportCommentAsync(string commentId)
        {
            var comment = await _unitOfWork.ReportCommentRepository.GetFirstOrDefaultAsync(c=>c.ReportCommentId == commentId);
            if (comment == null)
                return new ApiNotFoundResponse<object>($"Cannot found the comment with id: {commentId}");

            _unitOfWork.ReportCommentRepository.Delete(comment);

            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                var commentVm = new ReportCommentDto()
                {
                    ReportCommentId = comment.ReportCommentId,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    ReportId = comment.ReportId,
                    UpdatedAt = comment.UpdatedAt,
                    UserId = comment.UserId,
                };
                return new ApiResponse<object>(200, "Xóa thành công", commentVm);
            }
            return new ApiBadRequestResponse<object>($"Xóa comment thất bại");
        }
        
    }
}
