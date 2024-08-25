using Amazon.S3.Model.Internal.MarshallTransformations;
using KLTN.Application.DTOs.Comments;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Application.Services;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class CommentController : BaseController
    {
        private readonly CommentService commentService;
        public CommentController(ApplicationDbContext db,CommentService commentService)
        {
            this.commentService = commentService;
        }
        [HttpGet("{announcementId}/comments")]
        public async Task<IActionResult> GetCommentsPagingAsync(string announcementId,string filter, int pageIndex,int pageSize)
        {
            return SetResponse(await commentService.GetCommentsFromAnnouncementAsync(announcementId) );
        }
        
        [HttpPost("{announcementId}/comments")]
        [ApiValidationFilter]
        public async Task<IActionResult> PostComment(string announcementId, [FromBody] CreateCommentRequestDto request)
        {
            return SetResponse(await commentService.CreateCommentsAsync(announcementId, request));
        }

        [HttpPatch("{announcementId}/comments/{commentId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutComment(string commentId, [FromBody] CreateCommentRequestDto request)
        {
            return SetResponse(await commentService.UpdateCommentAsync(commentId, request));
        }

        [HttpDelete("{announcementId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(string announcementId, string commentId)
        {
            return SetResponse(await commentService.DeleteCommentAsync(commentId));
        }
    }
}
