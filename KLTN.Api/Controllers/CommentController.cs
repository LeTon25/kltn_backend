using KLTN.Application.DTOs.Comments;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
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
        public ApplicationDbContext _db;    
        public CommentController(ApplicationDbContext db)
        {
            _db = db;
        }
        [HttpGet("{announcementId}/comments/filter")]
        public async Task<IActionResult> GetCommentsPagingAsync(string announcementId,string filter, int pageIndex,int pageSize)
        {
            var query = from c in _db.Comments
                        join u in _db.Users
                            on c.UserId equals u.Id
                        select new { c, u };
            query = query.Where(x => x.c.AnnoucementId == announcementId);
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.c.Content.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.c.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentDto()
                {
                    CommentId = c.c.CommentId,
                    Content = c.c.Content,
                    CreatedAt = c.c.CreatedAt,
                    UpdatedAt = c.c.UpdatedAt,
                    OwnerUserId = c.c.UserId,
                    OwnerName = c.u.FullName
                })
                .ToListAsync();

            var pagination = new Pagination<CommentDto>
            {
                Items = items,
                TotalRecords = totalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(new ApiResponse<Pagination<CommentDto>>(200,"Thành công",pagination));
        }
        
        [HttpPost("{announcementId}/comments")]
        [ApiValidationFilter]
        public async Task<IActionResult> PostComment(string announcementId, [FromBody] CreateCommentRequestDto request)
        {
            var comment = new Comment()
            {
                Content = request.Content,
                AnnoucementId = announcementId,
                UserId = request.UserId,
                CreatedAt = DateTime.Now,
                CommentId = Guid.NewGuid().ToString(),
            };
            _db.Comments.Add(comment);
            var result = await _db.SaveChangesAsync();
            if (result > 0)
            {
               return Ok(new ApiResponse<string>(200,"Thành công"));
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse<string>("Bình luận thất bại"));
            }
        }

        [HttpPut("{announcementId}/comments/{commentId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutComment(int commentId, [FromBody] CreateCommentRequestDto request)
        {
            var comment = await _db.Comments.FindAsync(commentId);
            if (comment == null)
                return BadRequest(new ApiBadRequestResponse<string>($"Không thể tìm thấy bình luận với id : {commentId}"));
            if (comment.UserId != request.UserId)
                return Forbid("Không thể chỉnh sửa bình luận");

            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.Now;   
            _db.Comments.Update(comment);

            var result = await _db.SaveChangesAsync();

            if (result > 0)
            {
                return Ok(new ApiResponse<string>(200, "Cập nhật thành công"));
            }
            return BadRequest(new ApiBadRequestResponse<string>($"Update comment failed"));
        }

        [HttpDelete("{announcementId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int announcementId, int commentId)
        {
            var comment = await _db.Comments.FindAsync(commentId);
            if (comment == null)
                return NotFound(new ApiNotFoundResponse<string>($"Cannot found the comment with id: {commentId}"));

            _db.Comments.Remove(comment);

            var result = await _db.SaveChangesAsync();
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
                return Ok(new ApiResponse<CommentDto>(200,"Xóa thành công",commentVm));
            }
            return BadRequest(new ApiBadRequestResponse<string>($"Xóa comment thất bại"));
        }

        [HttpGet("comments/recent/{take}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecentComments(int take)
        {
                    var query = from c in _db.Comments
                            join u in _db.Users
                                on c.UserId equals u.Id
                            join k in _db.Announcements
                            on c.AnnoucementId equals k.AnnouncementId
                            orderby c.CreatedAt descending
                            select new { c, u, k };

                var comments = await query.Take(take).Select(x => new CommentDto()
                {
                    CommentId = x.c.CommentId,
                    CreatedAt = x.c.CreatedAt,
                    AnnouncementId = x.c.AnnoucementId,
                    OwnerUserId = x.c.UserId,
                    OwnerName = x.u.FullName,
                    Content = x.c.Content,
                }).ToListAsync();
            return Ok(new ApiResponse<List<CommentDto>>(200,"Thành công",comments));
        }
    }
}
