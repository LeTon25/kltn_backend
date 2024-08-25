using AutoMapper;
using KLTN.Application.DTOs.Announcements;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp;
using File = KLTN.Domain.Entities.File;

namespace KLTN.Application.Services
{

    public class AnnoucementService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly CommentService commentService;
        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;
        public AnnoucementService(IUnitOfWork unitOfWork,CommentService commentService,UserManager<User> userManager,IMapper mapper)
        {
            this.unitOfWork = unitOfWork;   
            this.commentService = commentService;
            this.userManager = userManager;
            this.mapper = mapper;
        }

        public async Task<ApiResponse<object>> TogglePinAnnouncement(string announcementId, bool isPinned)
        {
            var announcement = await unitOfWork.AnnnouncementRepository.GetFirstOrDefault(c=>c.AnnouncementId == announcementId);
            if (announcement == null) 
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy thông báo");
            }
            announcement.IsPinned = isPinned;
            unitOfWork.AnnnouncementRepository.Update(announcement);
            await unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Thành công");
        }
        public async Task<ApiResponse<object>> GetAnnouncementByIdAsync(string annoucementId)
        {
            var data = await GetAnnoucementDtoByIdAsync(annoucementId);
            if(data == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy thông báo");
            }
            return new ApiResponse<object>(200, "Thành công", data);
        }
        public async Task<ApiResponse<object>> DeleteAnnouncementAsync(string annoucementId)
        {
            var subject = await unitOfWork.AnnnouncementRepository.GetFirstOrDefault(c => c.AnnouncementId == annoucementId);
            if (subject == null)
            {
                return new ApiNotFoundResponse<object>("Không thể tìm thấy thông báo với id");
            }
            unitOfWork.AnnnouncementRepository.Delete(subject);
            var result = await unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Thành công", mapper.Map<AnnouncementDto>(subject));
            }
            return new ApiBadRequestResponse<object>("Xóa thông tin thông báo thất bại");
        }
        public async Task<ApiResponse<object>> UpdateAnnouncementAsync(string announcementId, CreateAnnouncementRequestDto requestDto)
        {
            var announcement = await unitOfWork.AnnnouncementRepository.GetFirstOrDefault(c => c.AnnouncementId == announcementId);
            if (announcement == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy thông báo");
            }
            announcement.Content = requestDto.Content;
            announcement.AttachedLinks = requestDto.AttachedLinks;
            announcement.UpdatedAt = DateTime.Now;
            announcement.Attachments = mapper.Map<List<KLTN.Domain.Entities.File>>(requestDto.Attachments);
            announcement.Mentions = requestDto.Mentions;
            announcement.IsPinned = requestDto.IsPinned;
            unitOfWork.AnnnouncementRepository.Update(announcement);
            var result = await unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Cập nhập thành công", mapper.Map<AnnouncementDto>(announcement));
            }
            return new ApiBadRequestResponse<object>("Cập nhật thông báo thất bại");
        }
        public async Task<ApiResponse<object>> CreateAnnouncementAsync(CreateAnnouncementRequestDto requestDto)
        {
            var newAnnouncementId = Guid.NewGuid();
            var newAnnouncement = new Announcement()
            {
                AnnouncementId = newAnnouncementId.ToString(),
                Content = requestDto.Content,
                UserId = requestDto.UserId,
                CourseId = requestDto.CourseId,
                AttachedLinks = requestDto.AttachedLinks,
                Attachments = mapper.Map<List<File>>(requestDto.Attachments),
                Mentions = requestDto.Mentions,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                IsPinned = requestDto.IsPinned,
            };
            await unitOfWork.AnnnouncementRepository.AddAsync(newAnnouncement);
            await unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Cập nhập thành công", mapper.Map<AnnouncementDto>(newAnnouncement));
        }
        
        public async Task<List<AnnouncementDto>> GetAnnouncementDtosInCourseAsync(string courseId)
        {
            var announcements = unitOfWork.AnnnouncementRepository.GetAll(c=>c.CourseId == courseId);
            var announcementDtos = mapper.Map<List<AnnouncementDto>>(announcements);
            for (int i = 0; i < announcementDtos.Count; i++) 
            {
                announcementDtos[i] = await GetAnnoucementDtoByIdAsync(announcementDtos[i].AnnouncementId);
            }
            return announcementDtos;
        }
        public async Task<AnnouncementDto> GetAnnoucementDtoByIdAsync(string annoucementId)
        {
            var announcementFromDb = await unitOfWork.AnnnouncementRepository.GetFirstOrDefault(c=>c.AnnouncementId == annoucementId);
            if (announcementFromDb == null)
            {
                return null;
            }
            var announcementDto = mapper.Map<AnnouncementDto>(announcementFromDb);
            var createUser = await userManager.FindByIdAsync(announcementDto.UserId);
            var commentDtos = await commentService.GetCommentDtosFromAnnoucementAsync(annoucementId);
            announcementDto.CreateUser = mapper.Map<UserDto>(createUser);
            announcementDto.Comments = commentDtos; 
            return announcementDto;
        }
    }
}
