﻿using AutoMapper;
using KLTN.Application.DTOs.Settings;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Repositories;

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
            var setting = await unitOfWork.SettingRepository.GetFirstOrDefaultAsync(c => c.SettingId.Equals(dto.SettingId), false, c => c.Course!);
            if(setting == null)
            {
                return new ApiNotFoundResponse<SettingDto>("Không tìm thấy cài đặt");
            }    
            if (currentUserId != setting.Course!.LecturerId)
            {
                return new ApiBadRequestResponse<SettingDto>("Bạn không có quyền thay đổi cài đặt");
            }
            var now = DateTime.Now;
            var isTimeChanged = setting.DueDateToJoinGroup != dto.DueDateToJoinGroup;
            setting.HasFinalScore = dto.HasFinalScore;
            setting.MaxGroupSize = dto.MaxGroupSize;
            setting.MinGroupSize = dto.MinGroupSize;
            setting.AllowStudentCreateProject = dto.AllowStudentCreateProject;
            if(isTimeChanged)
            {
                if(dto.DueDateToJoinGroup == null)
                {
                    setting.DueDateToJoinGroup = null;
                }
                else
                {
                    if (dto.DueDateToJoinGroup < DateTime.Now)
                    {
                        return new ApiBadRequestResponse<SettingDto>("Hạn tham gia nhóm không hợp lệ");
                    }
                    setting.DueDateToJoinGroup = dto.DueDateToJoinGroup.Value.AddHours(7);
                }  
             
               
            }    
            unitOfWork.SettingRepository.Update(setting);
            
            
            await unitOfWork.SaveChangesAsync();
            var updatedSetting = await unitOfWork.SettingRepository.GetFirstOrDefaultAsync(c => c.SettingId.Equals(dto.SettingId), false, c => c.Course!);
            var settingDto = mapper.Map<SettingDto>(updatedSetting);
            
            return new ApiResponse<SettingDto>(200,"Cập nhật cài đặt thành công",settingDto);

        }
    }
}
