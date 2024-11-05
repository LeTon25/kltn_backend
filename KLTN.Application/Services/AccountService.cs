using AutoMapper;
using KLTN.Application.DTOs.Accounts;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Requests;
using KLTN.Application.DTOs.Subjects;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Services
{
    public class AccountService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;
        public AccountService(IUnitOfWork unitOfWork,UserManager<User> userManager,IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.mapper = mapper;
        }
        public async Task<ApiResponse<List<RequestDto>>> GetRequestsByUserAsync(string userId)
        {
            var requests = await unitOfWork.RequestRepository.FindByCondition(c => c.UserId.Equals(userId), false, c => c.Group!, c => c.User!).ToListAsync();
            foreach(var item in requests)
            {
                item.Group!.Requests = null;
            }
            var dto = mapper.Map<List<RequestDto>>(requests);

            return new ApiResponse<List<RequestDto>>(200, "Lấy dữ liệu thành công",dto);
        }
        public async Task<ApiResponse<object>> GetCoursesByCurrentUserAsync(string userId)
        {
            var userData = await userManager.Users.ToListAsync();
            var subjectData = await unitOfWork.SubjectRepository.GetAllAsync();

            var teachingCourses = unitOfWork.CourseRepository.GetAll(c => c.LecturerId == userId && c.SaveAt == null).ToList();
            var teachingCourseDtos = mapper.Map<List<CourseDto>>(teachingCourses);

            foreach(var teachingCourseDto in teachingCourseDtos)
            {
                teachingCourseDto.Subject = mapper.Map<SubjectDto>(subjectData.FirstOrDefault(c=>c.SubjectId==teachingCourseDto.SubjectId));
                teachingCourseDto.Lecturer = mapper.Map<UserDto>(userData.FirstOrDefault(c=>c.Id == teachingCourseDto.SubjectId));
            }


            var enrollData = unitOfWork.EnrolledCourseRepository.GetAll(c => c.StudentId == userId);

            var enrollCourseIds = enrollData.Select(e => e.CourseId).ToList();
            var enrollCourses = unitOfWork.CourseRepository.GetAll(c => enrollCourseIds.Contains(c.CourseId) && c.SaveAt == null).ToList();

            var enrollCourseDto = mapper.Map<List<CourseDto>>(enrollCourses);
            foreach(var courseDto in enrollCourseDto)
            {
                courseDto.Subject = mapper.Map<SubjectDto>(subjectData.FirstOrDefault(c => c.SubjectId == courseDto.SubjectId));
                courseDto.Lecturer = mapper.Map<UserDto>(userData.FirstOrDefault(c => c.Id == courseDto.SubjectId));
            }
            return new ApiResponse<object>(200, "Thành công", new CourseByUserDto()
            {
                CreatedCourses = teachingCourseDtos,
                EnrolledCourses = enrollCourseDto,
            });
        }
        public async Task<ApiResponse<object>> GetArchivedCoursesByCurrentUserAsync(string userId)
        {
            var userData = await userManager.Users.ToListAsync();
            var subjectData = await unitOfWork.SubjectRepository.GetAllAsync();

            var data = new List<CourseDto>();

            var teachingCourses = await unitOfWork.CourseRepository.FindByCondition(c => c.LecturerId == userId && c.SaveAt != null).ToListAsync();
            var teachingCourseDtos = mapper.Map<List<CourseDto>>(teachingCourses);

            foreach (var teachingCourseDto in teachingCourseDtos)
            {
                teachingCourseDto.Subject = mapper.Map<SubjectDto>(subjectData.FirstOrDefault(c => c.SubjectId == teachingCourseDto.SubjectId));
                teachingCourseDto.Lecturer = mapper.Map<UserDto>(userData.FirstOrDefault(c => c.Id == teachingCourseDto.SubjectId));
            }

            data.AddRange(teachingCourseDtos);

            var enrollData = await unitOfWork.EnrolledCourseRepository.FindByCondition(c => c.StudentId == userId).ToListAsync();

            var enrollCourseIds = enrollData.Select(e => e.CourseId).ToList();
            var enrollCourses = await unitOfWork.CourseRepository.FindByCondition(c => enrollCourseIds.Contains(c.CourseId) && c.SaveAt != null).ToListAsync();

            var enrollCourseDto = mapper.Map<List<CourseDto>>(enrollCourses);
            foreach (var courseDto in enrollCourseDto)
            {
                courseDto.Subject = mapper.Map<SubjectDto>(subjectData.FirstOrDefault(c => c.SubjectId == courseDto.SubjectId));
                courseDto.Lecturer = mapper.Map<UserDto>(userData.FirstOrDefault(c => c.Id == courseDto.SubjectId));
            }

            data.AddRange(enrollCourseDto);
            return new ApiResponse<object>(200, "Thành công", new ArchivedCourseByUserDto()
            {
               ArchivedCourses = data 
            });
        }

    }
}
