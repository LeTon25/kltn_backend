using AutoMapper;
using KLTN.Application.DTOs.Reports;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Application.Services.HttpServices;
using KLTN.Domain.Entities;
using KLTN.Domain.Enums;
using KLTN.Domain.Extensions;
using KLTN.Domain.Repositories;
using KLTN.Domain.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using File = KLTN.Domain.Entities.File;

namespace KLTN.Application.Services
{

    public class ReportService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;
        private readonly CommentService commentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration configuration;
        private readonly BackgroundJobHttpService backgroundJobHttpService;
        public ReportService(IUnitOfWork unitOfWork,UserManager<User> userManager,IMapper mapper,
            CommentService commentService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            BackgroundJobHttpService backgroundJobHttpService
     )
        {
            this.unitOfWork = unitOfWork;   
            this.userManager = userManager;
            this.mapper = mapper;
            this.commentService = commentService;
            this._httpContextAccessor = httpContextAccessor;
            this.configuration = configuration;
            this.backgroundJobHttpService = backgroundJobHttpService;
        }
        #region for_controller
        public async Task<ApiResponse<List<ReportDto>>> GetAllReportsAsync()
        {
            var reports = await unitOfWork.ReportRepository.FindByCondition(c => true, false, c => c.Group,c => c.Brief).ToListAsync();
            var dto = mapper.Map<List<ReportDto>>(reports);

            return new ApiResponse<List<ReportDto>>(200, "Thành công", dto);
        }
        public async Task<ApiResponse<object>> GetReportByIdAsync(string reportId)
        {
            var entity = await unitOfWork.ReportRepository.GetFirstOrDefaultAsync(c=>c.ReportId.Equals(reportId),false,c=>c.CreateUser!,c =>c.Brief);
            if (entity == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy báo cáo");
            }
            var data = mapper.Map<ReportDto>(entity);
            data.Comments = await commentService.GetCommentDtosFromPostAsync(data.ReportId,CommentableType.Report);
            return new ApiResponse<object>(200, "Thành công", data);
        }
        public async Task<ApiResponse<object>> DeleteReportAsync(string reportId)
        {
            var entity = await unitOfWork.ReportRepository.GetFirstOrDefaultAsync(c => c.ReportId == reportId);
            if (entity == null)
            {
                return new ApiNotFoundResponse<object>("Không thể tìm thấy báo cáo");
            }
            unitOfWork.ReportRepository.Delete(entity);
            var result = await unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Thành công", mapper.Map<ReportDto>(entity));
            }
            return new ApiBadRequestResponse<object>("Xóa thông tin báo cáo thất bại");
        }
        public async Task<ApiResponse<object>> UpdateReportAsync(string reportId, CreateReportRequestDto requestDto)
        {
            var report = await unitOfWork.ReportRepository.GetFirstOrDefaultAsync(c => c.ReportId == reportId,false,c=>c.CreateUser);
            if (report == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy báo cáo");
            }
            report.Content = requestDto.Content;
            report.AttachedLinks = mapper.Map<List<MetaLinkData>>(requestDto.AttachedLinks);
            report.UpdatedAt = DateTime.Now;
            report.CreatedAt = requestDto.CreatedAt ?? report.CreatedAt; 
            report.Attachments = mapper.Map<List<KLTN.Domain.Entities.File>>(requestDto.Attachments);
            report.Title = requestDto.Title;
            unitOfWork.ReportRepository.Update(report);
            var result = await unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Cập nhập thành công", mapper.Map<ReportDto>(report));
            }
            return new ApiBadRequestResponse<object>("Cập nhật báo cáo thất bại");
        }
        public async Task<ApiResponse<object>> CreateReportAsync(string userId,CreateReportRequestDto requestDto)
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var newReportId = Guid.NewGuid();
            var newReport = new Report()
            {
                ReportId = newReportId.ToString(),
                Content = requestDto.Content,
                UserId = currentUserId,
                Title = requestDto.Title,
                GroupId = requestDto.GroupId,
                AttachedLinks = mapper.Map<List<MetaLinkData>>(requestDto.AttachedLinks),
                Attachments = mapper.Map<List<File>>(requestDto.Attachments),
                CreatedAt = requestDto.CreatedAt ?? DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
            };
            
            await unitOfWork.ReportRepository.AddAsync(newReport);
            await unitOfWork.SaveChangesAsync();
            
            
            var dto = mapper.Map<ReportDto>(newReport);
            var userEntity =  await userManager.FindByIdAsync(currentUserId);
            dto.CreateUser =  mapper.Map<UserDto>(userEntity) ;

            var group = await unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId.Equals(requestDto.GroupId), false, c => c.GroupMembers, c => c.Course!, c => c.Course!.Lecturer!,c => c.Assignment);
            await SendNotiAsync(group, newReportId.ToString(),currentUserId,requestDto.Title);
            return new ApiResponse<object>(200, "Cập nhập thành công",dto);
        }
        #endregion
        private async Task SendNotiAsync(Group group,string newReportId,string createUserId,string reportTitle)
        {
            try
            {
                var clientUrl = configuration.GetSection("ClientUrl").Value;
                var uri = "/api/schedule-jobs/send-noti";
                if (group.GroupMembers != null && group.GroupMembers.Any()) 
                {
                    var userIds = new List<string>();
                    userIds = group.GroupMembers.Select(c => c.StudentId).ToList();
                    if (!createUserId.Equals(group.Course!.LecturerId))
                    {
                        userIds.Remove(createUserId);
                        userIds.Add(group.Course!.LecturerId);
                    }
                    var users = await unitOfWork.UserRepository.FindByCondition(c => userIds.Contains(c.Id)).ToListAsync();
                    var userEmails = users.Where(c => c.Email != null && !c.Email.Contains("@example")).Select(c => c.Email!).ToList();
                    if (userEmails.Any()) 
                    {
                        var assignmentTitle = group.Assignment != null ? group.Assignment.Title : "Báo cáo cuối kì";
                        var model = new NotiEventDto
                        {
                            CourseName = group.Course.Name,
                            Title = $"Báo cáo mới - '{reportTitle}'",
                            Message = $"Một báo cáo mới vừa được tạo ở nhóm {group.GroupName} - {assignmentTitle}",
                            ObjectLink = $"{clientUrl}/groups/{group.CourseId}/{group.GroupId}/reports/",
                            Emails = userEmails
                        };
                        var response = await backgroundJobHttpService.Client.PostAsJson(uri, model);
                    }
                }
            }
            catch
            {

            }
        }

    }
}
