using AutoMapper;
using KLTN.Api.Services.Interfaces;
using KLTN.Application.DTOs.Announcements;
using KLTN.Application.DTOs.Uploads;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.Diagnostics.Eventing.Reader;
using System.Net.Mail;
using System.Net.WebSockets;
using File = KLTN.Domain.Entities.File;

namespace KLTN.Api.Controllers
{
    [Authorize]
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
            var query = from announcement in _db.Announcements
                        join user in _db.Users on announcement.UserId equals user.Id into announcementUsers
                        from user in announcementUsers.DefaultIfEmpty()
                        select new AnnouncementDto
                        {
                            AnnouncementId= announcement.AnnouncementId,
                            UserId=announcement.UserId,
                            CourseId=announcement.CourseId,
                            Content=announcement.Content,
                            AttachedLinks=announcement.AttachedLinks,
                            Attachments= _mapper.Map<List<FileDto>>(announcement.Attachments),
                            CreatedAt=announcement.CreatedAt,
                            UpdatedAt=announcement.UpdatedAt,
                            DeletedAt=announcement.DeletedAt,
                            CreateUser = _mapper.Map<UserDto>(user)
                        };
            var announcementDtos = await query.ToListAsync();
            return Ok(new ApiResponse<List<AnnouncementDto>>(200,"Thành công",announcementDtos));
        }
        [HttpGet("filter")]
        public async Task<IActionResult> GetAnnouncementsPagingAsync(string filter,int pageIndex,int pageSize)
        {
            var query = from announcement in _db.Announcements
                        join user in _db.Users on announcement.UserId equals user.Id into announcementUsers
                        from user in announcementUsers.DefaultIfEmpty()
                        select new AnnouncementDto
                        {
                            AnnouncementId = announcement.AnnouncementId,
                            UserId = announcement.UserId,
                            CourseId = announcement.CourseId,
                            Content = announcement.Content,
                            AttachedLinks = announcement.AttachedLinks,
                            Attachments =_mapper.Map<List<FileDto>>(announcement.Attachments),
                            CreatedAt = announcement.CreatedAt,
                            UpdatedAt = announcement.UpdatedAt,
                            DeletedAt = announcement.DeletedAt,
                            CreateUser = _mapper.Map<UserDto>(user)
                        };
            var totalRecords = await query.CountAsync();
            var items= await query.Skip((pageIndex-1)*pageSize).Take(pageSize).ToListAsync();
            var data = items.Select(e => _mapper.Map<AnnouncementDto>(e)).ToList();

            var pagination = new Pagination<AnnouncementDto>
            {
                Items = data,
                TotalRecords = totalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(new ApiResponse<Pagination<AnnouncementDto>>(200,"Thành công",pagination));
        }
        [HttpPatch("{announcementId}/pinned")]
        public async Task<IActionResult> PatchAnnouncementsAsync(string announcementId,bool isPinned)
        {
            var query = from an in _db.Announcements where an.AnnouncementId == announcementId select an ;
            var currentAnnouncement = await query.FirstOrDefaultAsync();
            if (currentAnnouncement == null) 
            { 
                return NotFound(new ApiNotFoundResponse<string>("Không tìm thấy thông báo"));  
            }

            currentAnnouncement.IsPinned = isPinned;
            _db.Announcements.Update(currentAnnouncement);
            await _db.SaveChangesAsync();

            return Ok(new ApiResponse<string>(200, "Thành công"));
            
        }
        [HttpGet("{announcementId}")]
        public async Task<IActionResult> GetByIdAsync(string announcementId)
        {
            var query = from announcement in _db.Announcements where announcement.AnnouncementId == announcementId
                        join user in _db.Users on announcement.UserId equals user.Id into announcementUsers
                        from user in announcementUsers.DefaultIfEmpty()
                        select new AnnouncementDto
                        {
                            AnnouncementId = announcement.AnnouncementId,
                            UserId = announcement.UserId,
                            CourseId = announcement.CourseId,
                            Content = announcement.Content,
                            AttachedLinks = announcement.AttachedLinks,
                            Attachments = _mapper.Map<List<FileDto>>(announcement.Attachments),
                            CreatedAt = announcement.CreatedAt,
                            UpdatedAt = announcement.UpdatedAt,
                            DeletedAt = announcement.DeletedAt,
                            CreateUser = _mapper.Map<UserDto>(user)
                        };
            if (await query.CountAsync() == 0)
            {
                return NotFound(new ApiNotFoundResponse<string>("Không tìm thấy thông báo cần tìm"));
            }
            return Ok(_mapper.Map<AnnouncementDto>(await query.FirstOrDefaultAsync()));  

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
                Attachments = _mapper.Map<List<File>>(requestDto.Attachments),
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                IsPinned = requestDto.IsPinned,
            };
            await _db.AddAsync(newAnnouncement);
            await _db.SaveChangesAsync();
            return Ok(_mapper.Map<AnnouncementDto>(newAnnouncement));
        }
        [HttpPut("{announcementId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutAnnouncementId(string announcementId, [FromBody] CreateAnnouncementRequestDto requestDto)
        {
            var announcement = await _db.Announcements.FirstOrDefaultAsync(c=>c.AnnouncementId == announcementId);
            if(announcement == null)
            {
                return NotFound(new ApiNotFoundResponse<string>("Không tìm thấy thông báo"));
            }
            announcement.Content = requestDto.Content;
            announcement.AttachedLinks = requestDto.AttachedLinks;
            announcement.UpdatedAt = DateTime.Now;
            announcement.IsPinned = requestDto.IsPinned;
            _db.Announcements.Update(announcement);
            var result = await _db.SaveChangesAsync();
            if(result > 0)
            {
                return Ok(new ApiResponse<string>(200,"Cập nhập thành công")); 
            }
            return BadRequest(new ApiBadRequestResponse<string>("Cập nhật thông báo thất bại"));
        }

        [HttpDelete("{announcementId}")]
        public async Task<IActionResult> DeleteAnnouncementAsync(string announcementId)
        {
            var announcement = await _db.Announcements.FirstOrDefaultAsync(c => c.AnnouncementId == announcementId);
            if (announcement == null) {
                return NotFound(new ApiNotFoundResponse<string>("Không thể tìm thấy thông báo với id"));
            }
            _db.Announcements.Remove(announcement);
            var result = await _db.SaveChangesAsync();
            if (result > 0) {
                return Ok(_mapper.Map<AnnouncementDto>(announcement));
            }
            return BadRequest(new ApiBadRequestResponse<string>("Xóa thông tin thông báo thất bại"));
        }

    }
}
