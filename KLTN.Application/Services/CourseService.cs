﻿using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KLTN.Domain.Entities;
using AutoMapper;
using KLTN.Application.Helpers.Response;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Users;
using KLTN.Application.DTOs.Subjects;
using KLTN.Application.DTOs.Semesters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using KLTN.Application.DTOs.Projects;
using Microsoft.EntityFrameworkCore;
namespace KLTN.Application.Services
{
    public class CourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IMapper mapper;
        private readonly AnnoucementService annoucementService;
        public CourseService(IUnitOfWork unitOfWork,UserManager<User> userManager,IMapper mapper,AnnoucementService annoucementService) 
        { 
            this._unitOfWork = unitOfWork;
            this._userManager = userManager;
            this.mapper = mapper;  
            this.annoucementService = annoucementService;
        }
        #region for controller
        public async Task<ApiResponse<object>> GetAllCoursesAsync()
        {
            var courses = await _unitOfWork.CourseRepository.GetAllAsync();
            var courseDtos = mapper.Map<List<CourseDto>>(courses.ToList());
            foreach(var courseDto in courseDtos)
            {
                var lecturer = mapper.Map<UserDto>(await _userManager.FindByIdAsync(courseDto.LecturerId)) ;
                var subject = mapper.Map<SubjectDto>(await _unitOfWork.SubjectRepository.GetFirstOrDefault(c=>c.SubjectId == courseDto.SubjectId));
                var semester = mapper.Map<SemesterDto>(await _unitOfWork.SemesterRepository.GetFirstOrDefault(c => c.SemesterId == courseDto.SemesterId));
                courseDto.Lecturer = lecturer;
                courseDto.Semester = semester;
                courseDto.Subject = subject;
            }
            return new ApiResponse<object>(200, "Thành công", courseDtos);
        }
        public async Task<ApiResponse<object>> CreateCourseAsync(CreateCourseRequestDto requestDto)
        {

            if (await _unitOfWork.CourseRepository.AnyAsync(c => c.CourseGroup == requestDto.CourseGroup && c.SemesterId == requestDto.SemesterId && c.SubjectId == requestDto.SubjectId))
            {
                return new ApiBadRequestResponse<object>("Nhóm môn học được mở không được trùng");
            }    
            var newCourseId = Guid.NewGuid();
            var newCourse = new Course()
            {
                CourseId = newCourseId.ToString(),
                CourseGroup = requestDto.CourseGroup,
                EnableInvite = requestDto.EnableInvite,
                InviteCode = requestDto.InviteCode ?? GenerateRandomNumericString(6),
                LecturerId = requestDto.LecturerId,
                SemesterId = requestDto.SemesterId,
                SubjectId = requestDto.SubjectId,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
            };
            await _unitOfWork.CourseRepository.AddAsync(newCourse);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Thành công", mapper.Map<CourseDto>(newCourse));
        }
        public async Task<ApiResponse<object>> UpdateCourseAsync(string courseId,CreateCourseRequestDto requestDto)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy lớp");
            }
            if (await _unitOfWork.CourseRepository.AnyAsync(c => c.CourseGroup == requestDto.CourseGroup && c.SemesterId == requestDto.SemesterId && c.SubjectId == requestDto.SemesterId && c.CourseId != course.CourseId))
            {
                return new ApiBadRequestResponse<object>("Nhóm môn học được mở không được trùng");
            }
            course.CourseGroup = requestDto.CourseGroup;
            course.EnableInvite = requestDto.EnableInvite;
            course.InviteCode = requestDto.InviteCode;
            course.UpdatedAt = DateTime.Now;
            course.LecturerId = requestDto.LecturerId;
            course.SemesterId = requestDto.SemesterId;
            course.SubjectId = requestDto.SubjectId;

            _unitOfWork.CourseRepository.Update(course);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Cập nhật thành công", mapper.Map<CourseDto>(course));
            }
            return new ApiBadRequestResponse<object>("Cập nhật lớp học thất bại");
        }
        public async Task<ApiResponse<object>> UpdateInviteCodeAsync(string courseId,string inviteCode)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy lớp");
            }
            if (string.IsNullOrEmpty(inviteCode))
            {
                return new ApiBadRequestResponse<object>("Mã mời không được trống");
            }
            course.InviteCode = inviteCode;
            _unitOfWork.CourseRepository.Update(course);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Cập nhật mã mời thành công", mapper.Map<CourseDto>(course));
            }
            return new ApiBadRequestResponse<object>("Cập nhật mã mời thất bại");
        }
        public async Task<ApiResponse<object>> GetCourseByIdAsync(string courseId)
        {
            var data = await GetCourseDtoByIdAsync(courseId);
            if(data == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy khóa học");
            }
            return new ApiResponse<object>(200, "Thành công", data);
        }
        public async Task<ApiResponse<object>> ApplyInviteCodeAsync(string courseId,string inviteCode,string userId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy lớp học");
            }
            if (course.InviteCode != inviteCode)
            {
                return new ApiBadRequestResponse<object>("Mã lớp học không chính xác");
            }
            if (!await _unitOfWork.EnrolledCourseRepository.AnyAsync(c => c.CourseId == courseId && c.StudentId == userId))
            {
                await _unitOfWork.EnrolledCourseRepository.AddAsync(new EnrolledCourse()
                {
                    CourseId = courseId,
                    StudentId = userId,
                    CreatedAt = DateTime.Now,
                });
                var result = await _unitOfWork.SaveChangesAsync();
                if (result < 0)
                {
                    return new ApiBadRequestResponse<object>("Không thể tham gia lớp học");
                }
            }
             return await GetCourseByIdAsync(courseId);
        }

        public async Task<ApiResponse<object>> DeleteCourseAsync(string courseId)
        {
            var course = await  _unitOfWork.CourseRepository.GetFirstOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                return new ApiNotFoundResponse<object>($"Không thể tìm thấy lớp học với id {courseId}");
            }
            _unitOfWork.CourseRepository.Delete(course);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Xóa thành công", mapper.Map<CourseDto>(course));
            }
            return new ApiBadRequestResponse<object>("Xóa thông tin lớp học thất bại");
        }
        public async Task<ApiResponse<object>> GetProjectsInCourseAsync(string courseId)
        {
            var projects = _unitOfWork.ProjectRepository.GetAll(c=>c.CourseId == courseId);
            var projectDtos = mapper.Map<List<ProjectDto>>(projects.ToList());

            foreach(var projectDto in projectDtos)
            {
                projectDto.CreateUser = mapper.Map<UserDto>(await _userManager.FindByIdAsync(projectDto.CreateUserId));
            }
            return new ApiResponse<object>(200, "Thành công", projectDtos);
        }
        #endregion
        public async Task<CourseDto> GetCourseDtoByIdAsync(string courseId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                return null;
            }
            var courseDto = mapper.Map<CourseDto>(course);
            courseDto.Semester = mapper.Map<SemesterDto>(await _unitOfWork.SemesterRepository.GetFirstOrDefault(c => c.SemesterId == courseDto.SemesterId));
            courseDto.Subject = mapper.Map<SubjectDto>(await _unitOfWork.SubjectRepository.GetFirstOrDefault(c => c.SubjectId == courseDto.SubjectId));
            courseDto.Lecturer = mapper.Map<UserDto>(await _userManager.FindByIdAsync(courseDto.LecturerId));
            courseDto.Announcements = await annoucementService.GetAnnouncementDtosInCourseAsync(courseId);

            var enrolledData = await _unitOfWork.EnrolledCourseRepository.GetAllAsync();
            var usersData = await _userManager.Users.ToListAsync();
            var users = from user in  usersData
                           join enroll in enrolledData on user.Id equals enroll.StudentId where enroll.CourseId == courseId
                           select user;
            courseDto.Students = mapper.Map<List<UserDto>>(users.ToList());
            return courseDto;

        }
        private string GenerateRandomNumericString(int length)
        {
            Random random = new Random();
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = (char)('0' + random.Next(0, 10));
            }

            return new string(result);
        }
    }
}