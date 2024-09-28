using AutoMapper;
using KLTN.Application.DTOs.Reports;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Enums;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp;
using File = KLTN.Domain.Entities.File;

namespace KLTN.Application.Services
{

    public class ReportService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;
        private readonly CommentService commentService;
        public ReportService(IUnitOfWork unitOfWork,UserManager<User> userManager,IMapper mapper,
            CommentService commentService)
        {
            this.unitOfWork = unitOfWork;   
            this.userManager = userManager;
            this.mapper = mapper;
            this.commentService = commentService;
        }
        #region for_controller
        public async Task<ApiResponse<object>> TogglePinReport(string reportId, bool isPinned)
        {
            var report = await unitOfWork.ReportRepository.GetFirstOrDefaultAsync(c => c.ReportId == reportId);
            if (report == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy báo cáo");
            }
            report.IsPinned = isPinned;
            unitOfWork.ReportRepository.Update(report);
            await unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Thành công");
        }
        public async Task<ApiResponse<object>> GetReportByIdAsync(string reportId)
        {
            var entity = await unitOfWork.ReportRepository.GetFirstOrDefaultAsync(c=>c.ReportId.Equals(reportId),false,c=>c.CreateUser);
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
            var report = await unitOfWork.ReportRepository.GetFirstOrDefaultAsync(c => c.ReportId == reportId);
            if (report == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy báo cáo");
            }
            report.Content = requestDto.Content;
            report.AttachedLinks = mapper.Map<List<MetaLinkData>>(requestDto.AttachedLinks);
            report.UpdatedAt = DateTime.Now;
            report.Attachments = mapper.Map<List<KLTN.Domain.Entities.File>>(requestDto.Attachments);
            report.Mentions = requestDto.Mentions;
            report.IsPinned = requestDto.IsPinned;
            report.DueDate = requestDto.DueDate;    
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
            var newReportId = Guid.NewGuid();
            var newReport = new Report()
            {
                ReportId = newReportId.ToString(),
                Content = requestDto.Content,
                UserId = userId,
                GroupId = requestDto.GroupId,
                AttachedLinks = mapper.Map<List<MetaLinkData>>(requestDto.AttachedLinks),
                Attachments = mapper.Map<List<File>>(requestDto.Attachments),
                Mentions = requestDto.Mentions,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                DueDate = requestDto.DueDate,
                IsPinned = requestDto.IsPinned,
            };
            await unitOfWork.ReportRepository.AddAsync(newReport);
            await unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Cập nhập thành công", mapper.Map<ReportDto>(newReport));
        }
        #endregion

    }
}
