using AutoMapper;
using KLTN.Application.DTOs.Comments;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Enums;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Services
{
    public class CommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<User> userManager;
        public CommentService(IUnitOfWork unitOfWork, IMapper mapper,UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
        }
        public async Task<ApiResponse<object>> GetAllCommentAsync()
        {
            var comments = await _unitOfWork.CommentRepository.FindByCondition(c => true, false, c => c.User).ToListAsync();
            var dto = mapper.Map<List<CommentDto>>(comments);
            return new ApiResponse<object>(200,"Thành công",dto);
        }
        public async Task<ApiResponse<object>> CreateCommentsAsync(string commentableId, CreateCommentRequestDto request,string userId)
        {
            string? commentableType =  null;
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return new ApiResponse<object>(400,"Không được bỏ trống nội dung bình luận");
            }
            if(await _unitOfWork.AnnnouncementRepository.AnyAsync(c=>c.AnnouncementId.Equals(commentableId)))
            {
                commentableType = CommentableType.Announcement;
            }
            if (string.IsNullOrEmpty(commentableType))
            {
                if (await _unitOfWork.ReportRepository.AnyAsync(c => c.ReportId.Equals(commentableId)))
                {
                    commentableType = CommentableType.Report;
                }
            }
            if (string.IsNullOrEmpty(commentableType))
            {
                if (await _unitOfWork.AssignmentRepository.AnyAsync(c => c.AssignmentId.Equals(commentableId)))
                {
                    commentableType = CommentableType.Assignment;
                }
            }
            if(string.IsNullOrEmpty(commentableType))
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy bài viết");
            }    
            var comment = new Comment()
            {
                Content = request.Content,
                UserId = userId,
                CommentableType = commentableType,
                CommentableId=commentableId,
                CreatedAt = DateTime.Now,
                CommentId = Guid.NewGuid().ToString(),
            };
            await _unitOfWork.CommentRepository.AddAsync(comment);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                var commentDto = mapper.Map<CommentDto>(comment);
                var user = await userManager.FindByIdAsync(userId);
                commentDto.User = mapper.Map<UserDto>(user);
                return new ApiResponse<object>(200, "Thành công",commentDto);
            }
            else
            {
                return new ApiBadRequestResponse<object>("Bình luận thất bại");
            }
        }
        public async Task<List<CommentDto>> GetCommentDtosFromPostAsync(string postId,string commentableType)
        {
            var comments =  _unitOfWork.CommentRepository.GetAll(c=>c.CommentableId == postId && c.CommentableType.Equals(commentableType)).ToList();
            var commentDtos =  mapper.Map<List<CommentDto>>(comments);

            foreach(var comment in commentDtos)
            {
                var user = await userManager.FindByIdAsync(comment.UserId);
                comment.User = mapper.Map<UserDto>(user);
            }    
            return commentDtos;
        }
        public async Task<ApiResponse<object>> UpdateCommentAsync(string commentId, CreateCommentRequestDto request,string userId)
        {
            var comment = await _unitOfWork.CommentRepository.GetFirstOrDefaultAsync(c=>c.CommentId == commentId);
            if (comment == null)
                return new ApiBadRequestResponse<object>($"Không thể tìm thấy bình luận với id : {commentId}");
            if (comment.UserId != userId)
                return new ApiResponse<object>(403,"Không thể cập nhật comment");
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return new ApiResponse<object>(400,"Không được bỏ trống nội dung bình luận");
            }
            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.Now;
            _unitOfWork.CommentRepository.Update(comment);

            var result = await _unitOfWork.SaveChangesAsync();

            if (result > 0)
            {
                var commentDto = mapper.Map<CommentDto>(comment);
                var user = await userManager.FindByIdAsync(userId);
                commentDto.User = mapper.Map<UserDto>(user);
                return new ApiResponse<object>(200, "Cập nhật thành công",commentDto);
            }
            return new ApiBadRequestResponse<object>($"Cập nhật thất bại");
        }
        public async Task<ApiResponse<object>> DeleteCommentAsync(string commentId)
        {
            var comment = await _unitOfWork.CommentRepository.GetFirstOrDefaultAsync(c=>c.CommentId == commentId);
            if (comment == null)
                return new ApiNotFoundResponse<object>($"Không thể tìm thấy comment với id : {commentId}");

            _unitOfWork.CommentRepository.Delete(comment);

            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                var commentVm = new CommentDto()
                {
                    CommentId = comment.CommentId,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    CommentableId = comment.CommentableId,
                    CommentableType = comment.CommentableType,
                    UpdatedAt = comment.UpdatedAt,
                    UserId = comment.UserId,
                };
                return new ApiResponse<object>(200, "Xóa thành công", commentVm);
            }
            return new ApiBadRequestResponse<object>($"Xóa comment thất bại");
        }
        
    }
}
