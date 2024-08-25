using AutoMapper;
using KLTN.Application.DTOs.Accounts;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Semesters;
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

        public async Task<ApiResponse<object>> GetCoursesByCurrentUserAsync(string userId)
        {
            var userData = await userManager.Users.ToListAsync();
            var semesterData = await unitOfWork.SemesterRepository.GetAllAsync();
            var subjectData = await unitOfWork.SubjectRepository.GetAllAsync();

            var teachingCourses = unitOfWork.CourseRepository.GetAll(c => c.LecturerId == userId).ToList();
            var teachingCourseDtos = mapper.Map<List<CourseDto>>(teachingCourses);

            foreach(var teachingCourseDto in teachingCourseDtos)
            {
                teachingCourseDto.Semester = mapper.Map<SemesterDto>(semesterData.FirstOrDefault(c=>c.SemesterId == teachingCourseDto.SemesterId));
                teachingCourseDto.Subject = mapper.Map<SubjectDto>(subjectData.FirstOrDefault(c=>c.SubjectId==teachingCourseDto.SubjectId));
                teachingCourseDto.Lecturer = mapper.Map<UserDto>(userData.FirstOrDefault(c=>c.Id == teachingCourseDto.SubjectId));
            }


            var enrollData = unitOfWork.EnrolledCourseRepository.GetAll(c => c.StudentId == userId);

            var enrollCourseIds = enrollData.Select(e => e.CourseId).ToList();
            var enrollCourses = unitOfWork.CourseRepository.GetAll(c => enrollCourseIds.Contains(c.CourseId)).ToList();

            var enrollCourseDto = mapper.Map<List<CourseDto>>(enrollCourses);
            foreach(var courseDto in enrollCourseDto)
            {
                courseDto.Semester = mapper.Map<SemesterDto>(semesterData.FirstOrDefault(c => c.SemesterId == courseDto.SemesterId));
                courseDto.Subject = mapper.Map<SubjectDto>(subjectData.FirstOrDefault(c => c.SubjectId == courseDto.SubjectId));
                courseDto.Lecturer = mapper.Map<UserDto>(userData.FirstOrDefault(c => c.Id == courseDto.SubjectId));
            }
            return new ApiResponse<object>(200, "Thành công", new CourseByUserDto()
            {
                CreatedCourses = teachingCourseDtos,
                EnrolledCourses = enrollCourseDto,
            });
        }
    }
}
