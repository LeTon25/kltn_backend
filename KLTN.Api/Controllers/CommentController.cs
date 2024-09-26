using Amazon.S3.Model.Internal.MarshallTransformations;
using KLTN.Application.DTOs.Comments;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        [HttpPost("{commentableId}/comments")]
        [ApiValidationFilter]
        public async Task<IActionResult> PostComment(string commentableId, [FromBody] CreateCommentRequestDto request)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await commentService.CreateCommentsAsync(commentableId, request,userId));
        }

        [HttpPatch("{commentableId}/comments/{commentId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutComment(string commentId, [FromBody] CreateCommentRequestDto request)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await commentService.UpdateCommentAsync(commentId, request,userId));
        }

        [HttpDelete("{commentableId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(string commentableId, string commentId)
        {
            return SetResponse(await commentService.DeleteCommentAsync(commentId));
        }
    }
}
