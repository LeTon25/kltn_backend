using AutoMapper;
using KLTN.Api.Services.Interfaces;
using KLTN.Application.DTOs.Announcements;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.Net.WebSockets;

namespace KLTN.Api.Controllers
{
    public class AnnouncementsController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;
        public AnnouncementsController(ApplicationDbContext db,
            IMapper mapper,
            IStorageService storageService
            ) {
            this._db = db;
            this._mapper = mapper;
            this._storageService = storageService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAnnouncementsAsync()
        {
            var query = _db.Announcements;

            var announcementDtos = await query.ToListAsync();
            return Ok(_mapper.Map<List<AnnouncementDto>>(announcementDtos));
        }
        [HttpGet("filter")]
        public async Task<IActionResult> GetAnnouncementsPagingAsync(string filter,int pageIndex,int pageSize)
        {
            var query = _db.Announcements.AsQueryable();
            
            var totalRecords = await query.CountAsync();
            var items= await query.Skip((pageIndex-1)*pageSize).Take(pageSize).ToListAsync();
            var data = items.Select(e => _mapper.Map<AnnouncementDto>(e)).ToList();

            var pagination = new Pagination<AnnouncementDto>
            {
                Items = data,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }
        [HttpGet("{announcementId}")]
        public async Task<IActionResult> GetByIdAsync(string announcementId)
        {
            var announcement = await _db.Announcements.FindAsync(announcementId);
            if(announcement == null)
            {
                return NotFound(new ApiNotFoundResponse("Không tìm thấy học kỳ cần tìm"));
            }
            return Ok(_mapper.Map<AnnouncementDto>(announcement));  

        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostAnnouncementAsync(CreateAnnouncementRequestDto requestDto)
        {
            var announcements = _db.Announcements;

            var newAnnouncementId = Guid.NewGuid();
            var newAnnouncement = new Announcement()
            {
                AnnouncementId = newAnnouncementId.ToString(),
                Content = requestDto.Content,
                UserId = requestDto.UserId,
                CourseId = requestDto.CourseId,
                AttachedLinks = requestDto.AttachedLinks,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
            };
            var result = await _db.AddAsync(newAnnouncement);
            await _db.SaveChangesAsync();
            return Ok(_mapper.Map<AnnouncementDto>(result));
        }
        [HttpPut("{announcementId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutAnnouncementId(string announcementId, [FromBody] CreateAnnouncementRequestDto requestDto)
        {
            var announcement = await _db.Announcements.FirstOrDefaultAsync(c=>c.AnnouncementId == announcementId);
            if(announcement == null)
            {
                return NotFound(new ApiNotFoundResponse("Không tìm thấy thông báo"));
            }
            announcement.Content = requestDto.Content;
            announcement.AttachedLinks = requestDto.AttachedLinks;
            announcement.UpdatedAt = DateTime.Now;

            _db.Announcements.Update(announcement);
            var result = await _db.SaveChangesAsync();
            if(result > 0)
            {
                return NoContent(); 
            }
            return BadRequest(new ApiBadRequestResponse("Cập nhật học kì thất bại"));
        }

        [HttpDelete("{announcementId}")]
        public async Task<IActionResult> DeleteAnnouncementAsync(string announcementId)
        {
            var announcement = await _db.Announcements.FirstOrDefaultAsync(c => c.AnnouncementId == announcementId);
            if (announcement == null) {
                return NotFound(new ApiNotFoundResponse("Không thể tìm thấy học kì với id"));
            }
            _db.Announcements.Remove(announcement);
            var result = await _db.SaveChangesAsync();
            if (result > 0) {
                return Ok(_mapper.Map<AnnouncementDto>(announcement));
            }
            return BadRequest(new ApiBadRequestResponse("Xóa thông tin học kì thất bại"));
        }

    }
}
