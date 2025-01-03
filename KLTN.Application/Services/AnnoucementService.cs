using AutoMapper;
using KLTN.Application.DTOs.Announcements;
using KLTN.Application.DTOs.Comments;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Application.Services.HttpServices;
using KLTN.Domain.Entities;
using KLTN.Domain.Extensions;
using KLTN.Domain.Repositories;
using KLTN.Domain.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using File = KLTN.Domain.Entities.File;

namespace KLTN.Application.Services
{

    public class AnnoucementService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly CommentService commentService;
        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly BackgroundJobHttpService backgroundJobHttpService;


        public AnnoucementService(IUnitOfWork unitOfWork,CommentService commentService,
            UserManager<User> userManager,IMapper mapper,IConfiguration configuration,
            BackgroundJobHttpService backgroundJobHttpService)
        {
            this.unitOfWork = unitOfWork;   
            this.commentService = commentService;
            this.userManager = userManager;
            this.mapper = mapper;
            this.configuration = configuration;
            this.backgroundJobHttpService = backgroundJobHttpService;
        }
        #region for_controller
        public async Task<ApiResponse<object>> TogglePinAnnouncement(string announcementId, bool isPinned)
        {
            var announcement = await unitOfWork.AnnnouncementRepository.GetFirstOrDefaultAsync(c => c.AnnouncementId == announcementId);
            if (announcement == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy thông báo");
            }
            announcement.IsPinned = isPinned;
            unitOfWork.AnnnouncementRepository.Update(announcement);
            await unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Thành công");
        }
        public async Task<ApiResponse<object>> GetAllAnnouncementsAsync()
        {
            var annoucements = await unitOfWork.AnnnouncementRepository.FindByCondition(c => true, false, c => c.CreateUser,c=>c.Course).ToListAsync();
            var annoucementDtos = mapper.Map<List<AnnouncementDto>>(annoucements);
            return new ApiResponse<object>(200, "Thành công",annoucementDtos);
        }
        public async Task<ApiResponse<object>> GetAnnouncementByIdAsync(string annoucementId)
        {
            var data = await GetAnnoucementDtoByIdAsync(annoucementId);
            if (data == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy thông báo");
            }
            return new ApiResponse<object>(200, "Thành công", data);
        }
        public async Task<ApiResponse<object>> DeleteAnnouncementAsync(string annoucementId)
        {
            var subject = await unitOfWork.AnnnouncementRepository.GetFirstOrDefaultAsync(c => c.AnnouncementId == annoucementId);
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
            var announcement = await unitOfWork.AnnnouncementRepository.GetFirstOrDefaultAsync(c => c.AnnouncementId == announcementId,false,c=>c.CreateUser!);
            if (announcement == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy thông báo");
            }
            announcement.Content = requestDto.Content;
            announcement.AttachedLinks = mapper.Map<List<MetaLinkData>>(requestDto.AttachedLinks);
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
                AttachedLinks = mapper.Map<List<MetaLinkData>>(requestDto.AttachedLinks),
                Attachments = mapper.Map<List<File>>(requestDto.Attachments),
                Mentions = requestDto.Mentions,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                IsPinned = requestDto.IsPinned,
            };
            await unitOfWork.AnnnouncementRepository.AddAsync(newAnnouncement);
            await unitOfWork.SaveChangesAsync();
            
            var dto = mapper.Map<AnnouncementDto>(newAnnouncement);
            var createUser = await unitOfWork.UserRepository.GetFirstOrDefaultAsync(c => c.Id.Equals(requestDto.UserId));
            dto.CreateUser = mapper.Map<UserDto>(createUser);

            var course = await unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId.Equals(requestDto.CourseId), false, c => c.EnrolledCourses);
            await SendNotiAsync(course, requestDto.UserId, newAnnouncement);
            return new ApiResponse<object>(200, "Tạo thành công", mapper.Map<AnnouncementDto>(dto));
        }
        #endregion
        #region for_service

        #endregion
        private async Task SendNotiAsync(Course course,string createUserId,Announcement announcement)
        {
            try
            {
                var clientUrl = configuration.GetSection("ClientUrl").Value;
                var uri = "/api/schedule-jobs/send-noti";
                var emailsToSend = new List<string>();
                
                if (course.EnrolledCourses != null && course.EnrolledCourses.Count > 0)
                {
                    var userIds = course.EnrolledCourses!.Where(c=>c.StudentId != createUserId).Select(c => c.StudentId).ToList();
                    if (announcement.Mentions.Count() > 0)
                    {
                        userIds.Clear();
                        userIds.AddRange(announcement.Mentions);
                    }    

                    if(!createUserId.Equals(course.LecturerId))
                    {
                        userIds.Remove(createUserId);
                        userIds.Add(course.LecturerId);
                    }    
                    var users = await unitOfWork.UserRepository.FindByCondition(c => userIds.Contains(c.Id)).ToListAsync();
                    var userEmails = users.Where(c => c.Email != null && !c.Email.Contains("@example")).Select(c => c.Email!).ToList();
                    
                    emailsToSend.AddRange(userEmails);
                }
                if (emailsToSend.Count > 0) 
                {
                    var model = new NotiEventDto
                    {
                        CourseName = course.Name,
                        Title = "Thông báo mới",
                        Message = $"Một thông báo mới vừa được tạo",
                        ObjectLink = $"{clientUrl}/courses/{course.CourseId}/",
                        Emails = emailsToSend
                    };
                    var response = await backgroundJobHttpService.Client.PostAsJson(uri, model);
                }
            }
            catch
            {

            }
        }
        public async Task<List<AnnouncementDto>> GetAnnouncementDtosInCourseAsync(string courseId)
        {
            var announcements = unitOfWork.AnnnouncementRepository.FindByCondition(c => c.CourseId.Equals(courseId), false, c => c.CreateUser!);
            var announcementDtos = mapper.Map<List<AnnouncementDto>>(announcements);

            var announcementIds = announcements.Select(a => a.AnnouncementId).ToList();
            
            var comments = await unitOfWork.CommentRepository.FindByCondition(c => announcementIds.Contains(c.CommentableId) , false, c => c.User!).ToListAsync();

            var commentDtos = mapper.Map<List<CommentDto>>(comments);

            for (int i = 0; i < announcementDtos.Count; i++)
            {
                announcementDtos[i].Comments = commentDtos.Where(c => c.CommentableId.Equals(announcementDtos[i].AnnouncementId)).ToList();
            }
            return announcementDtos;
        }
        public async Task<AnnouncementDto> GetAnnoucementDtoByIdAsync(string annoucementId)
        {
            var announcement = await unitOfWork.AnnnouncementRepository.GetFirstOrDefaultAsync(c => c.AnnouncementId.Equals(annoucementId), false, c => c.CreateUser!);
            var announcementDto = mapper.Map<AnnouncementDto>(announcement);
            var comments = await unitOfWork.CommentRepository.FindByCondition(c => c.CommentableId.Equals(announcementDto.AnnouncementId), false, c => c.User!).ToListAsync();
            announcementDto.Comments = mapper.Map<List<CommentDto>>(comments);
            return announcementDto;
        }
    }
}
