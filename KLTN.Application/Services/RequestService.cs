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
            var group = await _unitOfWork.GroupRepository.GetFirstOrDefaultAsync(c => c.GroupId.Equals(groupId), false, c => c.GroupMembers, c => c.Course!, c => c.Course.EnrolledCourses);
            
            if (group == null)
            {
                return new ApiNotFoundResponse<RequestDto>("Không tìm thấy nhóm");
            }

            if (group.GroupMembers.Any(c => c.StudentId.Equals(currentUserId)))
            {
                return new ApiBadRequestResponse<RequestDto>("Bạn đã tham gia nhóm rồi");
            }

            if (group.Course!.LecturerId == currentUserId)
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
            
            if(await _unitOfWork.RequestRepository.AnyAsync(c => c.GroupId.Equals(groupId)))
            {
                return new ApiBadRequestResponse<RequestDto>("Nhóm đã có người khác xin làm nhóm trưởng");
            }

            var requestsByUser = await _unitOfWork.RequestRepository.FindByCondition(c => c.UserId.Equals(currentUserId),false).ToListAsync();
            if (requestsByUser.Any(c=> otherGroup.Any(e=>e.GroupId.Equals(c.GroupId))))
            {
                return new ApiBadRequestResponse<RequestDto>("Không thể yêu cầu tham gia nhiều nhóm cùng một lúc");
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

            var data = await _unitOfWork.RequestRepository.GetFirstOrDefaultAsync(c => c.RequestId.Equals(newRequestId), false, c => c.User);
            return new ApiResponse<RequestDto>(200, "Tạo yêu cầu thành công", mapper.Map<RequestDto>(data));
        }
        public async Task<ApiResponse<RequestDto>> DeleteRequestToJoinAsync(string requestId, string currentUserId)
        {
            var request = await _unitOfWork.RequestRepository.GetFirstOrDefaultAsync(c => c.RequestId.Equals(requestId), false, c=>c.User ,c => c.Group, c => c.Group.GroupMembers, c => c.Group.Course);
            if (request == null)
            {
                return new ApiNotFoundResponse<RequestDto>("Không tìm thấy request");
            }
            var isLead = false;
            var groupByCurrentUser = request.Group.GroupMembers.Where(c => c.StudentId.Equals(currentUserId)).FirstOrDefault();
            if (groupByCurrentUser != null && groupByCurrentUser.IsLeader)
            {
                isLead = true;
            }
            if (currentUserId != request.UserId && isLead != true && request.Group.Course!.LecturerId != currentUserId)
            {
                return new ApiBadRequestResponse<RequestDto>("Không có quyền để bỏ yêu cầu");
            }
            _unitOfWork.RequestRepository.Delete(request);
            await _unitOfWork.SaveChangesAsync();
            var dto = mapper.Map<RequestDto>(request);
            return new ApiResponse<RequestDto>(200,"Bỏ yêu cầu thành công",dto);
        }
        public async Task<ApiResponse<RequestDto>> AcceptRequestToJoinAsync(string requestId, string currentUserId)
        {
            var request = await _unitOfWork.RequestRepository.GetFirstOrDefaultAsync(c => c.RequestId.Equals(requestId), false, c => c.Group, c => c.Group.GroupMembers, c => c.Group.Course);
            if (request == null)
            {
                return new ApiNotFoundResponse<RequestDto>("Không tìm thấy request");
            }
            var isLead = false;
            
            var groupByCurrentUser = request.Group.GroupMembers.Where(c => c.StudentId.Equals(currentUserId)).FirstOrDefault();
            
            if (groupByCurrentUser != null && groupByCurrentUser.IsLeader)
            {
                isLead = true;
            }
            
            if (isLead != true && request.Group.Course!.LecturerId != currentUserId)
            {
                return new ApiBadRequestResponse<RequestDto>("Không có quyền để duyệt yêu cầu");
            }
            var memberCount = request.Group.GroupMembers != null ? request.Group.GroupMembers.Count() : 0;
            if (memberCount + 1 > request.Group.NumberOfMembers)
            {
                return new ApiBadRequestResponse<RequestDto>("Nhóm đã đủ thành viên");
            }
            await _unitOfWork.GroupMemberRepository.AddAsync(new GroupMember()
            {
                StudentId = request.UserId,
                GroupId = request.GroupId,
                IsLeader = false,
                CreatedAt = DateTime.Now,
            }); 
            _unitOfWork.RequestRepository.Delete(request);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<RequestDto>(200, "Chấp thuận yêu cầu thành công", null);
        }
        public async Task<ApiResponse<List<RequestDto>>> GetRequestsByUserAsync(string currentUserId)
        {
            var requests = await  _unitOfWork.RequestRepository.FindByCondition(c => c.UserId == currentUserId,false,c=>c.Group).ToListAsync();
            var dto = mapper.Map<List<RequestDto>>(requests);
            return new ApiResponse<List<RequestDto>>(200, "Thành công", dto);
        }
    }
}
