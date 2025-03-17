using KLTN.Application.DTOs.Comments;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentController : ControllerBase
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
            var response = await commentService.CreateCommentsAsync(commentableId, request, userId!);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPatch("{commentableId}/comments/{commentId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutComment(string commentId, [FromBody] CreateCommentRequestDto request)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await commentService.UpdateCommentAsync(commentId, request, userId!);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{commentableId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(string commentableId, string commentId)
        {
            var response = await commentService.DeleteCommentAsync(commentId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
