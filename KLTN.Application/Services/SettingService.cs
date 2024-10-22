using AutoMapper;
using KLTN.Application.DTOs.Settings;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Services
{
    public class SettingService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public SettingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }
        public async Task<ApiResponse<SettingDto>> UpdateSettingForCourseAsync(string courseId,SettingDto dto,string currentUserId)
        {
            if (courseId != dto.CourseId)
            {
                return new ApiBadRequestResponse<SettingDto>("Yêu cầu không hợp lệ");
            }
            if(dto.StartGroupCreation != null && dto.EndGroupCreation != null && dto.EndGroupCreation <= dto.StartGroupCreation)
            {
                return new ApiBadRequestResponse<SettingDto>("Ngày cuối tạo nhóm phải lớn hơn ngày bắt đầu tạo nhóm");
            }
            if(dto.MinGroupSize != null && dto.MaxGroupSize != null && dto.MaxGroupSize < dto.MinGroupSize)
            {
                return new ApiBadRequestResponse<SettingDto>("Thành viên tối đa của nhóm phải lớn hơn hoặc bằng số thành viên tối thiểu");
            }
            var setting = await unitOfWork.SettingRepository.GetFirstOrDefaultAsync(c => c.SettingId.Equals(dto.SettingId), false, c => c.Course);
            if(setting == null)
            {
                return new ApiNotFoundResponse<SettingDto>("Không tìm thấy cài đặt");
            }    
            if (currentUserId != setting.Course!.LecturerId)
            {
                return new ApiBadRequestResponse<SettingDto>("Bạn không có quyền thay đổi cài đặt");
            }
            setting.StartGroupCreation = dto.StartGroupCreation;
            setting.EndGroupCreation = dto.EndGroupCreation;    
            setting.HasFinalScore = dto.HasFinalScore;
            setting.MaxGroupSize = dto.MaxGroupSize;
            setting.MinGroupSize = dto.MinGroupSize;
            setting.AllowGroupRegistration = dto.AllowGroupRegistration;
            setting.AllowStudentCreateProject= dto.AllowStudentCreateProject;
            unitOfWork.SettingRepository.Update(setting);
            await unitOfWork.SaveChangesAsync();

            return new ApiResponse<SettingDto>(200,"Cập nhật cài đặt thành công",mapper.Map<SettingDto>(setting));

        }
    }
}
