using AutoMapper;
using KLTN.Application.DTOs.Scores;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.Formats.Asn1.AsnWriter;


namespace KLTN.Application.Services
{
    public class ScoreServices
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;
        public ScoreServices(IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.mapper = mapper;
        }

        public async Task<ApiResponse<List<ScoreDto>>> ScoringSubmissionAsync(CreateScoreDto requestDto, string currentUserId)
        {
            var submission = await unitOfWork.SubmissionRepository.GetFirstOrDefaultAsync(c => c.SubmissionId.Equals(requestDto.SubmissionId), false, c => c.Assignment!, c => c.Assignment.Course!,c => c.Scores);
            if (submission == null)
            {
                return new ApiBadRequestResponse<List<ScoreDto>>("Không tìm thấy bài nộp");
            }
            if (currentUserId != submission.Assignment.Course!.LecturerId)
            {
                return new ApiBadRequestResponse<List<ScoreDto>>("Chỉ có giáo viên của lớp mới có quyền chấm điểm");
            }
            if (submission.Assignment.ScoreStructureId == null)
            {
                return new ApiBadRequestResponse<List<ScoreDto>>("Bài này không có chấm điểm");
            }
            if(submission.Scores == null || submission.Scores.Count == 0 )
            {
                var scoresToAdd = new List<Score>();
                if (!submission.Assignment.IsGroupAssigned)
                {
                    var newId = Guid.NewGuid().ToString();
                    scoresToAdd.Add(new Score
                    {
                        ScoreStructureId = submission.Assignment.ScoreStructureId,
                        SubmissionId = requestDto.SubmissionId,
                        UserId = submission.UserId,
                        Value = requestDto.Value,
                        Id = newId
                    });
                }
                else
                {
                    var groupsInCourse = await unitOfWork.GroupRepository.FindByCondition(c => c.CourseId.Equals(submission.Assignment.CourseId), false, c => c.GroupMembers).ToListAsync();

                    var groupByUser = groupsInCourse.Where(c => c.GroupMembers.Any(e => e.StudentId.Equals(submission.UserId))).FirstOrDefault();
                    if (groupByUser == null)
                    {
                        return new ApiBadRequestResponse<List<ScoreDto>>("Người nộp chưa tham gia vào nhóm nào");
                    }
                    foreach (var item in groupByUser.GroupMembers)
                    {
                        var newId = Guid.NewGuid().ToString();
                        scoresToAdd.Add(new Score
                        {
                            ScoreStructureId = submission.Assignment.ScoreStructureId,
                            SubmissionId = requestDto.SubmissionId,
                            UserId = item.StudentId,
                            Value = requestDto.Value,
                            Id = newId
                        });
                    }
                }
                await unitOfWork.ScoreRepository.AddRangeAsync(scoresToAdd);
                await unitOfWork.SaveChangesAsync();
                var responseData = new List<ScoreDto>();
                var scores = await unitOfWork.ScoreRepository.FindByCondition(c => c.SubmissionId.Equals(requestDto.SubmissionId), false, c => c.User).ToListAsync();

                responseData = mapper.Map<List<ScoreDto>>(scores.Where(c => scoresToAdd.Any(e => e.UserId.Equals(c.UserId))));

                return new ApiResponse<List<ScoreDto>>(200, "Chấm điểm thành công", responseData);
            }
            foreach (var score in submission.Scores)
            {
                score.Value = requestDto.Value;
            }
            unitOfWork.ScoreRepository.UpdateRange(submission.Scores);
            var result = await unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                var dto = mapper.Map<List<ScoreDto>>(submission.Scores);
                return new ApiResponse<List<ScoreDto>>(200, "Cập nhật điểm thành công", dto);
            }
            return new ApiBadRequestResponse<List<ScoreDto>>("Cập nhật điểm thất bại");

        }
        public async Task<ApiResponse<ScoreDto>> UpdateScoreAsync(UpdateScoreDto requestDto,string scoreId,string currentUserId)
        {
            var score = await unitOfWork.ScoreRepository.GetFirstOrDefaultAsync(c => c.Id.Equals(scoreId), false, c => c.Submission, c => c.Submission.Assignment,c => c.User);
            if(score == null)
            {
                return new ApiNotFoundResponse<ScoreDto>("Không tìm thấy cột điểm");
            }
            score.Value = requestDto.Value;
            unitOfWork.ScoreRepository.Update(score);
            await unitOfWork.SaveChangesAsync();

            var responseData = mapper.Map<ScoreDto>(score);
            return new ApiResponse<ScoreDto>(200,"Cập nhật thành công",responseData);
            
        }
    }
}
