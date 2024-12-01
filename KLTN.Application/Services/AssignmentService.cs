using AutoMapper;
using KLTN.Application.DTOs.Assignments;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.Groups;
using KLTN.Application.DTOs.ScoreStructures;
using KLTN.Application.DTOs.Submissions;
using KLTN.Application.DTOs.Users;
using KLTN.Application.Helpers.Response;
using KLTN.Application.Services.HttpServices;
using KLTN.Domain;
using KLTN.Domain.Entities;
using KLTN.Domain.Enums;
using KLTN.Domain.Extensions;
using KLTN.Domain.Repositories;
using KLTN.Domain.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using File = KLTN.Domain.Entities.File;
using Group = KLTN.Domain.Entities.Group;
namespace KLTN.Application.Services
{
    public class AssignmentService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly CourseService courseService;  
        private readonly UserManager<User> userManager;
        private readonly CommentService commentService;
        private readonly IConfiguration configuration;
        private readonly BackgroundJobHttpService backgroundJobHttpService;
        public AssignmentService(IUnitOfWork unitOfWork, 
            CourseService courseService, 
            UserManager<User> userManager, 
            CommentService commentService,
            IMapper mapper,
            IConfiguration configuration,
            BackgroundJobHttpService backgroundJobHttpService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager; 
            this.courseService = courseService;
            this.commentService = commentService; 
            this.configuration = configuration; 
            this.backgroundJobHttpService = backgroundJobHttpService;   
        }
        #region for_controller
        public async Task<ApiResponse<object>> GetAssignmentByIdAsync(string assignmentId,string currentUserId)
        {
            var data = await GetAssignmentDtoByIdAsync(assignmentId,currentUserId);
            if (data == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy bài tập");
            }
            return new ApiResponse<object>(200, "Thành công", data);
        }
        public async Task<ApiResponse<object>> DeleteAssignmentAsync(string userId,string assignmentId)
        {
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefaultAsync(c => c.AssignmentId == assignmentId,false,c=>c.Course!);
            if (assignment == null)
            {
                return new ApiNotFoundResponse<object>("Không thể tìm thấy bài tập với id");
            }
            if (assignment.Course!.LecturerId != userId)
            {
                return new ApiResponse<object>(403, "Bạn không có quyền xóa bài tập này", null);
            }
            //if(assignment.JobId != null)
            //{
            //    await TriggerDeleteSendEmailReminderAsync(assignment.JobId);    
            //}
            //
            var groups = unitOfWork.GroupRepository.FindByCondition(c =>c.AssignmentId != null && c.AssignmentId.Equals(assignment.AssignmentId));
            unitOfWork.GroupRepository.DeleteRange(groups);
            unitOfWork.AssignmentRepository.Delete(assignment);

            var result = await unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Thành công", mapper.Map<AssignmentDto>(assignment));
            }
            return new ApiBadRequestResponse<object>("Xóa thông tin bài tập thất bại");
        }
        public async Task<ApiResponse<object>> UpdateAssignmentAsync(string userId,string assignmentId, UpSertAssignmentRequestDto requestDto)
        {
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefaultAsync(c => c.AssignmentId == assignmentId,false,c=>c.Course!,c =>c.Course.EnrolledCourses);
            if (assignment == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy bài tập");
            }
            if (assignment.Course!.LecturerId != userId)
            {
                return new ApiResponse<object>(403, "Bạn không có quyền chỉnh sửa bài tập này", null);
            }
            if(requestDto.ScoreStructureId != null)
            {
                var scoreStructure = await unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c => c.Id.Equals(requestDto.ScoreStructureId), false, c => c.Children);
                if (scoreStructure == null || scoreStructure.Children.Count > 0)
                {
                    return new ApiBadRequestResponse<object>("Cột điểm không hợp lệ");
                }
                if (await unitOfWork.AssignmentRepository.AnyAsync(c => c.CourseId.Equals(requestDto.CourseId) && c.ScoreStructureId == requestDto.ScoreStructureId && c.AssignmentId != assignmentId))
                {
                    return new ApiBadRequestResponse<object>("Cột điểm đã được chấm bởi bài tập khác");
                }
                if (scoreStructure.ColumnName == Constants.Score.EndtermColumnName && !assignment.Course.Setting!.HasFinalScore)
                {
                    return new ApiBadRequestResponse<object>("Chưa bật điểm cuối kì cho lớp học");
                }
            }
            //string? jobId = null;
            //if (assignment.DueDate != requestDto.DueDate && assignment.Course.EnrolledCourses != null && assignment.Course.EnrolledCourses.Count > 0) 
            //{
            //    if(assignment.JobId != null)
            //    {
            //        await TriggerDeleteSendEmailReminderAsync(assignment.JobId);
            //    }    
            //    if(requestDto.DueDate != null)
            //    {
            //        var duration = requestDto.DueDate - DateTime.Now;
            //        if (duration.Value.TotalHours > 8)
            //        {
            //            jobId = await TriggerSendEmailReminderAsync(assignment.Course, requestDto.Title, requestDto.DueDate.Value);
            //        }
            //    }    

            //}
            assignment.Title = requestDto.Title;
            assignment.Content = requestDto.Content;
            assignment.DueDate = requestDto.DueDate;
            assignment.AttachedLinks = mapper.Map<List<MetaLinkData>>(requestDto.AttachedLinks);
            assignment.UpdatedAt = DateTime.Now;
            assignment.Attachments = mapper.Map<List<KLTN.Domain.Entities.File>>(requestDto.Attachments);
            assignment.ScoreStructureId = requestDto.ScoreStructureId;
            assignment.Type = requestDto.Type;
            //assignment.JobId = jobId != null ? jobId.ToString() : assignment.JobId;
            unitOfWork.AssignmentRepository.Update(assignment);
            var result = await unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                var responseDto = await GetAssignmentDtoByIdAsync(assignmentId, userId);
                return new ApiResponse<object>(200, "Cập nhập thành công", responseDto);
            }
            return new ApiBadRequestResponse<object>("Cập nhật bài tập thất bại");
        }
        public async Task<ApiResponse<object>> CreateAssignmentAsync(string userId,UpSertAssignmentRequestDto requestDto)
        {
            var course = await unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId.Equals(requestDto.CourseId), false, c => c.Setting!,c =>c.EnrolledCourses);
            ScoreStructure scoreStructure = new ScoreStructure();
            List<GroupDto> groups = new List<GroupDto>();
            if(course == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy lớp học");
            }
            if (!await courseService.CheckIsTeacherAsync(userId, course.CourseId))
            {
                return new ApiResponse<object>(403, "Bạn không có quyền tạo bài tập", null);
            }
            if (requestDto.ScoreStructureId != null)
            {
                scoreStructure = await unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c => c.Id.Equals(requestDto.ScoreStructureId), false, c => c.Children);

                if(scoreStructure == null || scoreStructure.Children?.Count > 0)
                {
                    return new ApiBadRequestResponse<object>("Cột điểm không hợp lệ");
                }
                if (await unitOfWork.AssignmentRepository.AnyAsync(c => c.CourseId.Equals(requestDto.CourseId) && c.ScoreStructureId == requestDto.ScoreStructureId))
                {
                    return new ApiBadRequestResponse<object>("Cột điểm đã được chấm bởi bài tập khác");
                }
            }
            //string? jobId = null;
            //if(requestDto.DueDate != null && course.EnrolledCourses != null && course.EnrolledCourses.Count > 0)
            //{
            //    var duration = requestDto.DueDate - DateTime.Now;
            //    if(duration.Value.TotalHours > 8)
            //    {
            //        jobId = await TriggerSendEmailReminderAsync(course,requestDto.Title,requestDto.DueDate.Value);
            //    }    
            //}    
            var newAssignmentId = Guid.NewGuid();
            var newAssignment = new Assignment()
            {
                AssignmentId = newAssignmentId.ToString(),
                Content = requestDto.Content,
                CourseId = requestDto.CourseId,
                Title = requestDto.Title,
                ScoreStructureId = requestDto.ScoreStructureId,
                DueDate = requestDto.DueDate,
                AttachedLinks = mapper.Map<List<MetaLinkData>>(requestDto.AttachedLinks),
                Attachments = mapper.Map<List<File>>(requestDto.Attachments),
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                Type = requestDto.Type,
                //JobId = jobId,  
                IsGroupAssigned = requestDto.IsGroupAssigned,
                IsIndividualSubmissionRequired = requestDto.IsIndividualSubmissionRequired ?? false
            };
            await unitOfWork.AssignmentRepository.AddAsync(newAssignment);
            
            if (requestDto.IsGroupAssigned)
            {

                if(requestDto.AssignmentOptions == null)
                {
                    return new ApiBadRequestResponse<object>("Vui lòng chọn các lựa chọn cho nhóm");
                }

                if (requestDto.AssignmentOptions.AutoGenerateCount != null)
                {
                    groups = await GenerateAutoGroupsAsync(course,newAssignmentId.ToString(),requestDto.AssignmentOptions.AutoGenerateCount.Value);
                }
                else if (requestDto.AssignmentOptions.UseFinalGroup.HasValue && requestDto.AssignmentOptions.UseFinalGroup == true)
                {
                   var finalGroups = await GetFinalGroupsInCourseAsync(course);
                   await AddGroupsToAssignmentsAsync(finalGroups,newAssignmentId.ToString());
                   groups = mapper.Map<List<GroupDto>>(finalGroups);
                }
            }
            await unitOfWork.SaveChangesAsync();

            var responseDto = mapper.Map<AssignmentDto>(newAssignment);
            
            responseDto.Course = mapper.Map<CourseDto>(course);
            responseDto.ScoreStructure = mapper.Map<ScoreStructureDto>(scoreStructure);
            responseDto.Groups = groups;

            var userEntity = await userManager.Users.FirstOrDefaultAsync(c => c.Id.Equals(userId));
            responseDto.CreateUser = mapper.Map<UserDto>(userEntity);

            return new ApiResponse<object>(200, "Tạo thành công", mapper.Map<AssignmentDto>(responseDto));
        }
        public async Task<ApiResponse<object>> GetSubmissionsInAssignmentsAsync(string userId,string assignmentId)
        {
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefaultAsync(c => c.AssignmentId.Equals(assignmentId), false, c => c.Course!,c => c.Course.EnrolledCourses,c => c.Groups);
            if(assignment == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy bài tập");
            }
            if(assignment.Course!.LecturerId != userId)
            {
                return new ApiBadRequestResponse<object>("Bạn không có quyền lấy danh sách bài nộp");
            }

            var usersInCourseIds = assignment.Course.EnrolledCourses.Select(c=>c.StudentId).ToList();
            var usersInCourse = await unitOfWork.UserRepository.FindByCondition(c=> usersInCourseIds.Contains(c.Id)).ToListAsync();
            
            var submissions = await unitOfWork.SubmissionRepository.FindByCondition(c=>c.AssignmentId.Equals(assignmentId),false,c=>c.CreateUser!,c => c.Scores).ToListAsync();
            var groupsInCourse = new List<Group>();  

            if (assignment.Type.Equals(Constants.AssignmentType.Final))
            { 
                groupsInCourse = await unitOfWork.GroupRepository.FindByCondition(c => c.CourseId.Equals(assignment.CourseId) && c.GroupType.Equals(Constants.GroupType.Final), false, c => c.GroupMembers).ToListAsync();
            }
            else
            {
                groupsInCourse = await unitOfWork.GroupRepository.FindByCondition(c => c.CourseId.Equals(assignment.CourseId) && c.GroupType.Equals(Constants.GroupType.Normal) && c.AssignmentId != null && c.AssignmentId.Equals(assignment.AssignmentId), false, c => c.GroupMembers).ToListAsync();
            }
            var responseData = new List<SubmissionUserDto>();
            foreach(var student in usersInCourse)
            {
                if (!assignment.IsGroupAssigned || (assignment.IsGroupAssigned
                    && assignment.IsIndividualSubmissionRequired)) 
                {
                    var submissionSubmitedByUser = submissions.FirstOrDefault(c => c.UserId.Equals(student.Id));
                    if (submissionSubmitedByUser != null)
                    {
                        var score = submissionSubmitedByUser.Scores.FirstOrDefault(c => c.UserId.Equals(student.Id));
                        responseData.Add(new SubmissionUserDto()
                        {
                            User = mapper.Map<UserDto>(student),
                            Submission = mapper.Map<SubmissionNoScoreDto>(submissionSubmitedByUser),
                            Score = score != null ? score.Value : null,
                        });
                    }
                    else
                    {
                        responseData.Add(new SubmissionUserDto()
                        {
                            User = mapper.Map<UserDto>(student),
                            Submission = null,
                            Score = null
                        });
                    }
                }
                else
                {
                    var groupByUser = groupsInCourse.FirstOrDefault(c => c.GroupMembers.Any(e => e.StudentId == student.Id));
                    if (groupByUser == null) 
                    {
                        responseData.Add(new SubmissionUserDto()
                        {
                            User = mapper.Map<UserDto>(student),
                            Submission = null,
                            Score = null,
                        });
                        continue;
                    }
                    var submissionInGroup = submissions.FirstOrDefault(c => groupByUser.GroupMembers.Any(e => e.StudentId.Equals(c.UserId)));
                    if(submissionInGroup == null)
                    {
                        responseData.Add(new SubmissionUserDto()
                        {
                            User = mapper.Map<UserDto>(student),
                            Submission = null,
                            Score = null,
                            GroupId = groupByUser.GroupId,
                            GroupName = groupByUser.GroupName
                        });
                        continue;
                    }
                    var score = submissionInGroup.Scores.FirstOrDefault(c => c.UserId.Equals(student.Id));
                    responseData.Add(new SubmissionUserDto()
                    {
                        User = mapper.Map<UserDto>(student),
                        Submission = mapper.Map<SubmissionNoScoreDto>(submissionInGroup),
                        Score = score != null ? score.Value : null,
                        GroupId = groupByUser.GroupId,
                        GroupName = groupByUser.GroupName,  

                    });
                }
            }
            return new ApiResponse<object>(200,"Lấy dữ liệu thành công", responseData);
        }
        #endregion
        #region for_service
        public async Task<AssignmentDto> GetAssignmentDtoByIdAsync(string assignmentId,string currentUserId)
        {
            var assignment = await unitOfWork.AssignmentRepository.GetFirstOrDefaultAsync(c => c.AssignmentId == assignmentId,false,c=>c.ScoreStructure,c => c.Groups,c => c.Course,c => c.Course.EnrolledCourses);
            if (assignment == null)
            {
                return null;
            }
            if (assignment.Course!.LecturerId != currentUserId 
                && !assignment.Course.EnrolledCourses.Any(e=>e.CourseId.Equals(assignment.Course.CourseId) && e.StudentId.Equals(currentUserId)))
            {
                return null;
            }
            var assignmentDto = mapper.Map<AssignmentDto>(assignment);
            assignmentDto.Course = await courseService.GetCourseDtoByIdAsync(assignmentDto.CourseId, false, false,false,false);

            var userEntity = await userManager.Users.FirstOrDefaultAsync(c => c.Id.Equals(assignmentDto.Course.LecturerId));
            assignmentDto.CreateUser = mapper.Map<UserDto>(userEntity);

            assignmentDto.Comments = await commentService.GetCommentDtosFromPostAsync(assignmentId, CommentableType.Assignment);
            if(currentUserId != assignmentDto.Course.LecturerId)
            {
                var submission = await unitOfWork.SubmissionRepository.GetFirstOrDefaultAsync(c => c.AssignmentId.Equals(assignment.AssignmentId) && c.UserId.Equals(currentUserId), false,c=>c.CreateUser);
                assignmentDto.Submission = mapper.Map<SubmissionDto>(submission);
            }
            if (currentUserId != assignmentDto.Course.LecturerId && assignmentDto.Groups != null)
            {
                var listGroupApproved = assignmentDto.Groups.Where(c => c.IsApproved==true).ToList();
                assignmentDto.Groups = listGroupApproved;
            }    
            if (assignmentDto.Groups != null)
            {
                var groupIds = assignmentDto.Groups.Select(c => c.GroupId);
                var groupmembers = await unitOfWork.GroupMemberRepository.FindByCondition(c=> groupIds.Contains(c.GroupId),false,c=>c.Member!).ToListAsync();

                foreach (var item in assignmentDto.Groups) 
                {
                    var membersByGroup = groupmembers.Where(c => c.GroupId.Equals(item.GroupId)).ToList();
                    
                    var membersByGroupDto = new List<GroupMemberDto>();
                    foreach(var member in membersByGroup)
                    {
                        var groupMemberDto = mapper.Map<GroupMemberDto>(member);
                        groupMemberDto.StudentObj = mapper.Map<UserDto>(member.Member);
                        membersByGroupDto.Add(groupMemberDto);
                    }
                    item.GroupMembers = membersByGroupDto;
          
                }
            }
            return assignmentDto;
        }
        private async Task<List<GroupDto>> GenerateAutoGroupsAsync(Course course, string assignmentId, int count)
        {
            var allGroupsInCourse = await unitOfWork.GroupRepository.FindByCondition(c => c.CourseId.Equals(course.CourseId) && c.AssignmentId.Equals(assignmentId) && c.GroupType.Equals(Constants.GroupType.Normal), false).ToListAsync();
            var totalGroup = allGroupsInCourse.Count;
            var newGroupsAdded = new List<GroupDto>();
            for (int i = 0; i < count; i++)
            {
                var index = totalGroup + i + 1;
                var newGroupName = $"Nhóm {index}";

                while (allGroupsInCourse.Any(e => e.GroupName.Equals(newGroupName)))
                {
                    index++;
                    newGroupName = $"Nhóm {index}";
                }
                var newGroup = new KLTN.Domain.Entities.Group()
                {
                    GroupId = Guid.NewGuid().ToString(),
                    GroupName = newGroupName,
                    ProjectId = null,
                    NumberOfMembers = course.Setting!.MaxGroupSize ?? 5,
                    CourseId = course.CourseId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = null,
                    DeletedAt = null,
                    IsApproved = true,
                    GroupType = Constants.GroupType.Normal,
                    AssignmentId = assignmentId,
                };
                await unitOfWork.GroupRepository.AddAsync(newGroup);
                allGroupsInCourse.Add(newGroup);
                newGroupsAdded.Add(mapper.Map<GroupDto>(newGroup));

            }
            foreach (var group in newGroupsAdded)
            {
                group.Course = mapper.Map<CourseDto>(course);
                group.GroupMembers = new List<GroupMemberDto>();
            }
            return newGroupsAdded;
        }
        private async Task<List<GroupDto>> AddGroupsToAssignmentsAsync(List<Group> groups,string assignmentId)
        {
            var newListGroups = new List<GroupDto>();
            foreach(var item in groups)
            {
                var newGroup  = new Group()
                {
                    GroupId = Guid.NewGuid().ToString(),    
                    GroupName = item.GroupName,
                    CourseId = item.CourseId,
                    ProjectId  = item.ProjectId,
                    IsApproved = item.IsApproved,
                    NumberOfMembers = item.NumberOfMembers,
                    AssignmentId = assignmentId,
                    GroupType = Constants.GroupType.Normal ,
                 
                };
                newGroup.GroupMembers = new List<GroupMember>();
                foreach(var groupMember in item.GroupMembers!)
                {
                    newGroup.GroupMembers.Add(new GroupMember
                    {
                         StudentId = groupMember.StudentId,
                         GroupId = Guid.NewGuid().ToString(),
                         IsLeader = groupMember.IsLeader,
                         CreatedAt =DateTime.Now,
                    });
                }
                await unitOfWork.GroupRepository.AddAsync(newGroup);
                foreach(var groupmember in newGroup.GroupMembers)
                {
                    await unitOfWork.GroupMemberRepository.AddAsync(groupmember);
                }
                newListGroups.Add(mapper.Map<GroupDto>(newGroup));
            }
            return newListGroups;
        }
        private async Task<List<Group>> GetFinalGroupsInCourseAsync(Course course)
        {
            var finalGroups = await unitOfWork.GroupRepository.FindByCondition(c => c.CourseId.Equals(course.CourseId) && c.GroupType.Equals(Constants.GroupType.Final),false).ToListAsync();
            var finalGroupsId = finalGroups.Select(c=>c.GroupId).ToList();
            var groupmembers = await unitOfWork.GroupMemberRepository.FindByCondition(c => finalGroupsId.Contains(c.GroupId), false, c => c.Member!).ToListAsync();

            foreach (var item in finalGroups)
            {
                var memberByGroup = groupmembers.Where(c => c.GroupId.Equals(item.GroupId)).ToList();
                item.GroupMembers = memberByGroup;
            }
            return finalGroups;
        }
        private async Task<string?> TriggerSendEmailReminderAsync(Course course,string assignmentTitle,DateTime dueDate)
        {
            try
            {
                var uri = "/api/schedule-jobs/assignment-due-date";
                
                var studentIds = course.EnrolledCourses!.Select(c => c.StudentId).ToList();
                var students = await unitOfWork.UserRepository.FindByCondition(c => studentIds.Contains(c.Id)).ToListAsync();
                var model = new AssignmentReminderDto()
                    {
                        CourseName = course.Name,
                        AssignmentTitle = assignmentTitle,
                        DueDate = dueDate,
                        Emails = students != null ? students.Select(c=>c.Email!).ToList() : new List<string>() ,
                    };
                    var response = await backgroundJobHttpService.Client.PostAsJson(uri, model);

                    if (response.EnsureSuccessStatusCode().IsSuccessStatusCode) 
                    {
                        var jobId = await response.ReadContentAs<string>();
                        if (!string.IsNullOrEmpty(jobId)) 
                        {
                            return jobId;
                        }  
                    }
                return null;
                
            }
            catch
            {
                return null;
            }
        }
        private async Task TriggerDeleteSendEmailReminderAsync(string jobId)
        {
            try
            {
                var uri = $"/api/schedule-jobs/{jobId}";
                await backgroundJobHttpService.Client.DeleteAsync(uri);
            }
            catch
            {
            }
        }
        #endregion

    }
}
