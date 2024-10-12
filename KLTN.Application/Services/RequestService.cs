using AutoMapper;
using KLTN.Application.DTOs.Requests;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Services
{
    public class RequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper mapper;
        public RequestService(IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            this._unitOfWork = unitOfWork;
            this.mapper = mapper;
        }
        public async Task<ApiResponse<RequestDto>> MakeRequestToJoinAsync(string groupId, string currentUserId)
        {
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId.Equals(groupId), false, c => c.GroupMembers, c => c.Course, c => c.Course.EnrolledCourses);
            if (group == null)
            {
                return new ApiNotFoundResponse<RequestDto>("Không tìm thấy nhóm");
            }

            if (group.GroupMembers.Any(c => c.StudentId.Equals(currentUserId)))
            {
                return new ApiBadRequestResponse<RequestDto>("Bạn đã tham gia nhóm rồi");
            }

            if (group.Course.LecturerId == currentUserId)
            {
                return new ApiBadRequestResponse<RequestDto>("Giáo viên không cần phải yêu cầu tham gia nhóm");
            }

            if (!group.Course.EnrolledCourses.Any(c => c.StudentId.Equals(currentUserId)))
            {
                return new ApiBadRequestResponse<RequestDto>("Bạn chưa tham gia lớp học");
            }
            var otherGroup = await _unitOfWork.GroupRepository
                .FindByCondition(c => c.CourseId.Equals(group.CourseId) && c.GroupId != groupId, false, c => c.GroupMembers).ToListAsync();
            if (otherGroup.Any(c => c.GroupMembers
                            .Any(e => e.StudentId.Equals(currentUserId))))
            {
                return new ApiBadRequestResponse<RequestDto>("Bạn đã  tham gia nhóm khác");
            }


            var newRequestId = Guid.NewGuid().ToString();
            var newRequest = new Request()
            {
                RequestId = newRequestId,
                GroupId = groupId,
                UserId = currentUserId,
                CreatedAt = DateTime.Now,
            };
            await _unitOfWork.RequestRepository.AddAsync(newRequest);
            await _unitOfWork.SaveChangesAsync();

            var data = _unitOfWork.RequestRepository.GetFirstOrDefaultAsync(c => c.RequestId.Equals(newRequestId), false, c => c.User);
            return new ApiResponse<RequestDto>(200, "Tạo yêu cầu thành công", mapper.Map<RequestDto>(data));
        }

        //public async Task<ApiResponse<RequestDto>> DeleteRequestToJoinAsync(string requestId,string currentUserId)
        //{
        //    var request = await _unitOfWork.ReportRepository.GetFirstOrDefaultAsync(c => c.ReportId.Equals(requestId),false,c=>c.Group,c => c.Group.GroupMembers,c => c.Group.Course);
        //    if (request == null) 
        //    {
        //        return new ApiNotFoundResponse<RequestDto>("Không tìm thấy request");
        //    }
        //    var isLead = false;
        //    var groupByCurrentUser = request.Group.GroupMembers.Where(c => c.StudentId.Equals(currentUserId)).FirstOrDefault();
        //    if(groupByCurrentUser != null && groupByCurrentUser.IsLeader)
        //    {
        //        isLead = true;   
        //    }    
        //    if(currentUserId == request.UserId || isLead == true || request.Group.Course)
        //}
    }
}
