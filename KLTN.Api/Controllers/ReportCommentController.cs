using Amazon.S3.Model.Internal.MarshallTransformations;
using KLTN.Application.DTOs.ReportComments;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Services;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KLTN.Api.Controllers
{
    [Authorize]
    public class ReportCommentController : BaseController
    {
        private readonly ReportCommentService commentService;
        public ReportCommentController(ApplicationDbContext db,ReportCommentService commentService)
        {
            this.commentService = commentService;
        }
        [HttpPost("{reportId}/comments")]
        [ApiValidationFilter]
        public async Task<IActionResult> PostReportComment(string reportId, [FromBody] CreateReportCommentRequestDto request)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await commentService.CreateReportCommentsAsync(reportId, request,userId));
        }

        [HttpPatch("{reportId}/comments/{commentId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutReportComment(string commentId, [FromBody] CreateReportCommentRequestDto request)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return SetResponse(await commentService.UpdateReportCommentAsync(commentId, request,userId));
        }

        [HttpDelete("{reportId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteReportComment(string reportId, string commentId)
        {
            return SetResponse(await commentService.DeleteReportCommentAsync(commentId));
        }
    }
}
