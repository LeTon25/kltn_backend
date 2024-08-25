using AutoMapper;
using KLTN.Application.DTOs.Comments;
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

        public async Task<ApiResponse<object>> GetCommentsFromAnnouncementAsync(string announcementId)
        {
            
            var data = await GetCommentDtosFromAnnoucementAsync(announcementId); 
            return new ApiResponse<object>(200, "Thành công",data);
        }
        public async Task<ApiResponse<object>> CreateCommentsAsync(string announcementId, CreateCommentRequestDto request)
        {

            var comment = new Comment()
            {
                Content = request.Content,
                AnnoucementId = announcementId,
                UserId = request.UserId,
                CreatedAt = DateTime.Now,
                CommentId = Guid.NewGuid().ToString(),
            };
            await _unitOfWork.CommentRepository.AddAsync(comment);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Thành công",comment);
            }
            else
            {
                return new ApiBadRequestResponse<object>("Bình luận thất bại");
            }
        }
        public async Task<List<CommentDto>> GetCommentDtosFromAnnoucementAsync(string annoucementId)
        {
            var comments =  _unitOfWork.CommentRepository.GetAll(c=>c.AnnoucementId == annoucementId).ToList();
            var commentDtos =  mapper.Map<List<CommentDto>>(comments);

            foreach(var comment in commentDtos)
            {
                var user = await userManager.FindByIdAsync(comment.OwnerUserId);
                comment.User = mapper.Map<UserDto>(user);
            }    
            return commentDtos;
        }
        public async Task<ApiResponse<object>> UpdateCommentAsync(string commentId, CreateCommentRequestDto request)
        {
            var comment = await _unitOfWork.CommentRepository.GetFirstOrDefault(c=>c.CommentId == commentId);
            if (comment == null)
                return new ApiBadRequestResponse<object>($"Không thể tìm thấy bình luận với id : {commentId}");
            if (comment.UserId != request.UserId)
                return new ApiResponse<object>(403,"Không thể cập nhật comment");

            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.Now;
            _unitOfWork.CommentRepository.Update(comment);

            var result = await _unitOfWork.SaveChangesAsync();

            if (result > 0)
            {
                return new ApiResponse<object>(200, "Cập nhật thành công");
            }
            return new ApiBadRequestResponse<object>($"Update comment failed");
        }
        public async Task<ApiResponse<object>> DeleteCommentAsync(string commentId)
        {
            var comment = await _unitOfWork.CommentRepository.GetFirstOrDefault(c=>c.CommentId == commentId);
            if (comment == null)
                return new ApiNotFoundResponse<object>($"Cannot found the comment with id: {commentId}");

            _unitOfWork.CommentRepository.Delete(comment);

            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                var commentVm = new CommentDto()
                {
                    CommentId = comment.CommentId,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    AnnouncementId = comment.AnnoucementId,
                    UpdatedAt = comment.UpdatedAt,
                    OwnerUserId = comment.UserId,
                };
                return new ApiResponse<object>(200, "Xóa thành công", commentVm);
            }
            return new ApiBadRequestResponse<object>($"Xóa comment thất bại");
        }
        
    }
}
