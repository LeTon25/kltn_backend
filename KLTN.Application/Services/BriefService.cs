﻿using AutoMapper;
using KLTN.Application.DTOs.Briefs;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Services
{
    public class BriefService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        public BriefService(IUnitOfWork unitOfWork,IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<ApiResponse<BriefDto>> CreateBriefAsync(CreateBriefDto requestDto,string groupId,string currentUserId)
        {
            var group = await unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId.Equals(groupId),false,c=>c.Course);
            if (group == null)
            {
                return new ApiNotFoundResponse<BriefDto>("Không tìm thấy nhóm để thêm báo cáo");
            } 
            if(currentUserId != group.Course.LecturerId)
            {
                return new ApiBadRequestResponse<BriefDto>("Chỉ giáo viên mới có quyền được tạo bản tóm tắt");
            }    
            var newId = Guid.NewGuid().ToString();
            var newBrief = new Brief()
            {
                GroupId = groupId,
                Id = newId,
                CreatedAt = DateTime.Now,
                Content = requestDto.Content
            };
            await unitOfWork.BriefRepository.AddAsync(newBrief);
            await unitOfWork.SaveChangesAsync();

            return new ApiResponse<BriefDto>(200, "Tạo thành công", mapper.Map<BriefDto>(newBrief));
        }
        public async Task<ApiResponse<BriefDto>> UpdateBriefAsync(CreateBriefDto requestDto,string briefId ,string groupId, string currentUserId)
        {
            var group = await unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId.Equals(groupId), false, c => c.Course);
            if (group == null)
            {
                return new ApiNotFoundResponse<BriefDto>("Không tìm thấy nhóm để thêm báo cáo");
            }
            if (currentUserId != group.Course.LecturerId)
            {
                return new ApiBadRequestResponse<BriefDto>("Chỉ giáo viên mới có quyền được tạo bản tóm tắt");
            }
            var brief = await unitOfWork.BriefRepository.GetFirstOrDefaultAsync(c=>c.Id.Equals(briefId));

            if (brief == null) 
            { 
                return new ApiNotFoundResponse<BriefDto>("Không tìm thấy bản tóm tắt");
            }
            brief.Content = requestDto.Content;
            brief.UpdatedAt = DateTime.Now;

            unitOfWork.BriefRepository.Update(brief);
            await unitOfWork.SaveChangesAsync();  
            
            return new ApiResponse<BriefDto>(200, "Cập nhật thành công", mapper.Map<BriefDto>(brief));
        }
        public async Task<ApiResponse<BriefDto>> DeleteBriefAsync(string briefId, string groupId, string currentUserId)
        {
            var group = await unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId.Equals(groupId), false, c => c.Course);
            if (group == null)
            {
                return new ApiNotFoundResponse<BriefDto>("Không tìm thấy nhóm để thêm tóm tắt");
            }
            if (currentUserId != group.Course.LecturerId)
            {
                return new ApiBadRequestResponse<BriefDto>("Chỉ giáo viên mới có quyền được tạo bản tóm tắt");
            }
            var brief = await unitOfWork.BriefRepository.GetFirstOrDefaultAsync(c => c.Id.Equals(briefId));

            if (brief == null)
            {
                return new ApiNotFoundResponse<BriefDto>("Không tìm thấy bản tóm tắt");
            }
            unitOfWork.BriefRepository.Delete(brief);
            await unitOfWork.SaveChangesAsync();

            return new ApiResponse<BriefDto>(200, "Xóa thành công", null);
        }
        public async Task<ApiResponse<BriefDto>> GetBriefByIdAsync(string briefId,string currentUserId)
        {
            var brief = await unitOfWork.BriefRepository.GetFirstOrDefaultAsync(c=>c.Id.Equals(briefId),false,c=>c.Group,c => c.Group.Course);
            if (!brief.Group.Course.LecturerId.Equals(currentUserId))
            {
                return new ApiBadRequestResponse<BriefDto>("Chỉ giáo viên mới có quyền");
            }
            var dto = mapper.Map<BriefDto>(brief);
            return new ApiResponse<BriefDto>(200, "Lấy dữ liệu thành công", dto);
        }
        public async Task<ApiResponse<List<BriefDto>>> GetBriefsInGroupAsync(string groupId,string currentUserId)
        {
            var group = await unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId.Equals(groupId), false, c => c.Course);
            if (group == null)
            {
                return new ApiNotFoundResponse<List<BriefDto>>("Không tìm thấy nhóm để lấy bản tóm tắt");
            }
            if (currentUserId != group.Course.LecturerId)
            {
                return new ApiBadRequestResponse<List<BriefDto>>("Chỉ giáo viên mới có quyền được tạo bản tóm tắt");
            }
            var briefs = await unitOfWork.BriefRepository.FindByCondition(c => c.GroupId.Equals(groupId)).ToListAsync();
            var dto = mapper.Map<List<BriefDto>>(briefs);

            return new ApiResponse<List<BriefDto>>(200, "Thành công", dto);
        }
    }
}