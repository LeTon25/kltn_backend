﻿using AutoMapper;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.DTOs.ScoreStructures;
using KLTN.Application.Helpers.Response;
using KLTN.Domain;
using KLTN.Domain.Entities;
using KLTN.Domain.Exceptions;
using KLTN.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Application.Services
{
    public class ScoreStructureService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ScoreStructureService(IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork; 
            _mapper = mapper;
        }
        #region for_controller
        public async Task<ApiResponse<object>> SaveScoreStrucutureAsync(string userId,UpSertScoreStructureDto requestDto)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c=>c.CourseId.Equals(requestDto.CourseId),false);
            if (course == null) { 
                return new ApiNotFoundResponse<object>("Không tìm thấy lớp học");
            }
            if(course.LecturerId != userId)
            {
                return new ApiResponse<object>(403, "Chỉ có giảng viên mới có quyền cập nhật cấu trúc cột điểm");
            }
            var check  = ValidateColumnNameDuplicate(requestDto);
            if(check == false)
            {
                return new ApiResponse<object>(403, "Xuất hiện cột điểm có tên bị trùng");
            }
            var existingEntity = await _unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c=>c.Id.Equals(requestDto.Id),false);
            // Cập nhật thực thể đã tồn tại
            existingEntity.ColumnName = requestDto.ColumnName;
            existingEntity.Percent = requestDto.Percent;
            existingEntity.ParentId = requestDto.ParentId;

            await LoadChildrenAsync(existingEntity);
            // Cập nhật các children (nếu có)
            await UpdateChildren(existingEntity, requestDto.Children);

            _unitOfWork.ScoreStructureRepository.Update(existingEntity);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Thành công", _mapper.Map<ScoreStructureDto>(existingEntity));
        }

        public async Task<ApiResponse<object>> GetScoreStructureByIdAsync(string id)
        {
            var existingEntity = await _unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c => c.Id.Equals(id), false);
            if (existingEntity == null) { 
                return new ApiNotFoundResponse<object>("Không tìm thấy cột điểm");
            }
            await LoadChildrenAsync(existingEntity);
            var data = _mapper.Map<ScoreStructureDto>(existingEntity);
            data.Children = data.Children!.OrderByDescending(c => c.ColumnName).ToList();

            return new ApiResponse<object>(200, "Lấy dữ liệu thành công", data);
        }
        public async Task<ApiResponse<object>> GetScoreStructureByCourseIdAsync(string courseId)
        {
            var existingEntity = await _unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c => c.CourseId.Equals(courseId), false);
            if (existingEntity == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy cột điểm");
            }
            await LoadChildrenAsync(existingEntity);
            var data = _mapper.Map<ScoreStructureDto>(existingEntity);
            return new ApiResponse<object>(200, "Lấy dữ liệu thành công", data);
        }

        public async Task<ApiResponse<object>> GetTransciptAsync(string courseId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId.Equals(courseId) , false, c => c.EnrolledCourses,c => c.Setting);
            if (course == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy lớp học");
            }    
            var scoreStructure = await _unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c => c.CourseId.Equals(courseId)&& c.ColumnName.Equals(Constants.Score.FinalColumnName), false,c=>c.Scores);
            if (scoreStructure == null) 
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy cột điểm");
            }
            await LoadChildrenWithScoreAsync(scoreStructure);
            if (! course.Setting!.HasFinalScore)
            {
                var endtermScore = scoreStructure.Children!.FirstOrDefault(c=>c.ColumnName.Equals(Constants.Score.EndtermColumnName));
                scoreStructure.Children!.Remove(endtermScore!);
            }
            scoreStructure.Children = scoreStructure.Children!.OrderByDescending(c => c.ColumnName).ToList();   
            var studentIds = course.EnrolledCourses.Select(c=>c.StudentId).ToList();
            var students = await _unitOfWork.UserRepository.FindByCondition(c => studentIds.Contains(c.Id),false,c=>c.Scores).ToListAsync();

            var studentScores = students.Select(student => new StudentScoreDTO
            {
                Id = student.Id,    
                FullName = student.UserName!,
                Scores = CalculateScore(scoreStructure,student.Id)
            }).ToList();
            return new ApiResponse<object>(200, "Thành công", studentScores);
        }
        public async Task<ApiResponse<TranscriptStatisticsDto>> GetTransciptStatisticsAsync(string courseId)
        {
            var course = await _unitOfWork.CourseRepository.GetFirstOrDefaultAsync(c => c.CourseId.Equals(courseId), false, c => c.EnrolledCourses, c => c.Setting!);
            if (course == null)
            {
                return new ApiNotFoundResponse<TranscriptStatisticsDto>("Không tìm thấy lớp học");
            }
            if (course.EnrolledCourses != null && course.EnrolledCourses.Count() > 0) 
            {
                var scoreStructure = await _unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c => c.CourseId!.Equals(courseId) && c.ColumnName.Equals(Constants.Score.FinalColumnName), false, c => c.Scores);
                await LoadChildrenWithScoreAsync(scoreStructure);

                if (!course.Setting!.HasFinalScore)
                {
                    var endtermScore = scoreStructure.Children!.FirstOrDefault(c => c.ColumnName.Equals(Constants.Score.EndtermColumnName));
                    scoreStructure.Children!.Remove(endtermScore!);
                }
                scoreStructure.Children = scoreStructure.Children!.OrderByDescending(c => c.ColumnName).ToList();
                var studentIds = course.EnrolledCourses.Select(c => c.StudentId).ToList();
                var students = await _unitOfWork.UserRepository.FindByCondition(c => studentIds.Contains(c.Id), false, c => c.Scores).ToListAsync();

                var studentScores = students.Select(student => new StudentScoreDTO
                {
                    Id = student.Id,
                    FullName = student.UserName!,
                    Scores = CalculateScore(scoreStructure, student.Id)
                }).ToList();
                var statisticsDto = CalculateStatistics(studentScores, course.Setting.HasFinalScore);
                return new ApiResponse<TranscriptStatisticsDto>(200, "Thành công", statisticsDto);
            }
            return new ApiResponse<TranscriptStatisticsDto>(200, "Thành công", null);
        }
        #endregion
        private bool ValidateColumnNameDuplicate(UpSertScoreStructureDto dto)
        {
            var existedColumnName = new List<string>(); 
            existedColumnName.Add(dto.ColumnName);
            foreach (var child in dto.Children)
            {
               var isValid =  CheckValidCoulumnName(child, existedColumnName);
                if (!isValid)
                {
                    return false;
                }
            }  
            return true;
        }
        private bool CheckValidCoulumnName(ScoreStructureDto dto , List<string> existedColumnName)
        {
            if(existedColumnName.Contains(dto.ColumnName))
            {
                return false;
            }    
            existedColumnName.Add(dto.ColumnName);  

            if(dto.Children != null  || dto.Children.Count > 0 )
            {
                foreach (var child in dto.Children)
                {
                    var isValid = CheckValidCoulumnName(child, existedColumnName);
                    if(!isValid)
                    {
                        return false;
                    }    
                }
            }    
            return true;
        }
        private void SetParentIdForChildren(string parentId, ICollection<ScoreStructure> children)
        {
            foreach (var child in children)
            {
                if (string.IsNullOrEmpty(child.Id))
                {
                    child.Id = Guid.NewGuid().ToString(); // Tạo Guid mới cho cột con
                }

                child.ParentId = parentId;
                
                // Nếu cột con có cột con của nó, thì tiếp tục thiết lập ParentId cho chúng
                if (child.Children != null && child.Children.Any())
                {
                    SetParentIdForChildren(child.Id, child.Children);
                }
            }
        }
        private async Task UpdateChildren(ScoreStructure parentEntity, List<ScoreStructureDto>? children)
        {
            
            if (children == null || children.Count == 0)
            {
                if (parentEntity.Children != null && parentEntity.Children.Any())
                {
                    _unitOfWork.ScoreStructureRepository.DeleteRange(parentEntity.Children);
                    parentEntity.Children.Clear();
                }
                return;
            }
            if(parentEntity.Children != null)
            {
                var newChildIds = children.Select(c => c.Id).ToList();
                var childrenToRemove = parentEntity.Children
                    .Where(existingChild => !newChildIds.Contains(existingChild.Id))
                    .ToList();
                if (childrenToRemove.Any())
                {
                    _unitOfWork.ScoreStructureRepository.DeleteRange(childrenToRemove);
                    foreach (var child in childrenToRemove)
                    {
                        parentEntity.Children.Remove(child);
                    }
                }
            }
            var assignmentByScore = await _unitOfWork.AssignmentRepository.FindByCondition(c=> c.ScoreStructureId.Equals(parentEntity.Id),false).FirstOrDefaultAsync();

            foreach (var child in children)
            {

                var existingChild = parentEntity.Children?.FirstOrDefault(c=>c.Id.Equals(child.Id));
                if (existingChild == null)
                {
                    if(assignmentByScore != null)
                    {
                        throw new SplitScoreStructureException($"Không thể tách cột '{parentEntity.ColumnName}' do đã giao bài tập");
                    }    
                    if (string.IsNullOrEmpty(child.Id))
                    {
                        child.Id = Guid.NewGuid().ToString();
                    }
                    var newChild = _mapper.Map<ScoreStructure>(child);
                    newChild.ParentId = parentEntity.Id;
                    newChild.CourseId = parentEntity.CourseId;
                    if(newChild.Children != null && newChild.Children.Any())
                    {
                        SetParentIdForChildren(newChild.Id, newChild.Children);
                    }
                    if(parentEntity.Children == null)
                    {
                        parentEntity.Children = new List<ScoreStructure>();
                    }    
                    parentEntity.Children?.Add(newChild);
                    
                    // Kiểm tra xem phần trăm của các cột con có vượt quá cột cha không
                    if(parentEntity.Children != null)
                    {
                        var childrenPercent = children.Sum(e => e.Percent);
                        if (childrenPercent > parentEntity.Percent)
                            throw new InvalidScorePercentException("Phần trăm của các cột con không được lớn hơn cột cha");
                    }    
                    await _unitOfWork.ScoreStructureRepository.AddAsync(newChild);
                }
                else
                {
                    existingChild.ColumnName = child.ColumnName;
                    existingChild.Percent = child.Percent;
                    existingChild.CourseId = child.CourseId;
                    // Kiểm tra xem phần trăm của các cột con có vượt quá cột cha không
                    if (parentEntity.Children != null)
                    {
                        var childrenPercent = children.Sum(e=>e.Percent);
                        if (childrenPercent > parentEntity.Percent)
                            throw new InvalidScorePercentException("Phần trăm của các cột con không được lớn hơn cột cha");
                    }
                    await UpdateChildren(existingChild, child.Children);
                }
            }
        }
        // Hàm đệ quy tính điểm cho mỗi ScoreStructure, bao gồm các ScoreStructure con
        private ScoreDetailDto? CalculateScore(ScoreStructure scoreStructure, string studentId)
        {
            var scoreDetail = new ScoreDetailDto
            {
                ScoreStructureId = scoreStructure.Id,
                ColumnName = scoreStructure.ColumnName,
                Percent = scoreStructure.Percent,
            };
            if (scoreStructure.Children == null || !scoreStructure.Children.Any())
            {
                var scoreEntity = scoreStructure.Scores.Where(c => c.UserId.Equals(studentId)).FirstOrDefault();
                scoreDetail.Value = scoreEntity != null ? scoreEntity.Value : 0;
                scoreDetail.ScoreId = scoreStructure.Id;    
                
            }
            else
            {
                double? totalScore = 0;
                foreach (var child in scoreStructure.Children)
                {
                    var childScoreDetail = CalculateScore(child, studentId);
                    scoreDetail.Children.Add(childScoreDetail);  
                    if (childScoreDetail.Value.HasValue)
                    {
                        var percent  = (child.Percent / scoreDetail.Percent);
                        totalScore += (childScoreDetail.Value * percent);
                    }
                }
                scoreDetail.Value = totalScore;
            }
            return scoreDetail;
        }
        public async Task  LoadChildrenAsync(ScoreStructure parent)
        {
            var children = await _unitOfWork.ScoreStructureRepository
                    .FindByCondition(s => s.ParentId == parent.Id)
                    .AsNoTracking()
                    .ToListAsync();
            foreach (var child in children)
            {
                if(parent.Children == null)
                {
                    parent.Children = new List<ScoreStructure>();
                }    
                parent.Children.Add(child);
                await LoadChildrenAsync(child);
            }
        }
        private async Task LoadChildrenWithScoreAsync(ScoreStructure parent)
        {
            var children = await _unitOfWork.ScoreStructureRepository
                    .FindByCondition(s => s.ParentId == parent.Id,false,c=>c.Scores)
                    .AsNoTracking()
                    .ToListAsync();
            foreach (var child in children)
            {
                if (parent.Children == null)
                {
                    parent.Children = new List<ScoreStructure>();
                }
                parent.Children.Add(child);
                await LoadChildrenWithScoreAsync(child);
            }
        }
        private TranscriptStatisticsDto CalculateStatistics(List<StudentScoreDTO> studentScores,bool hasFinalScore)
        {
            var statistics = new TranscriptStatisticsDto();
            // Thống kê điểm quá trình
            var midtermScores = new  List<double>();
                foreach (var item in studentScores) 
                {
                    var midtermScore = item.Scores.Children.FirstOrDefault(c => c.ColumnName.Equals(Constants.Score.MidtermColumnName));

                    midtermScores.Add(midtermScore!.Value ?? 0);
                }
            var midtermStatistic = new ColumnStatistics();
            midtermStatistic.Distribution = CalculateScoreDistribution(midtermScores);
            midtermStatistic.Max = midtermScores.Max();
            midtermStatistic.Min = midtermScores.Min(); 
            midtermStatistic.Average  = Math.Round(midtermScores.Average(),2);
            midtermStatistic.ColumnName = Constants.Score.MidtermColumnName;

            statistics.ColumnStatistics.Add(midtermStatistic);  

            //Thống kê điểm đồ án cuối kì
            if (hasFinalScore)
            {
                var endtermScores = new List<double>();
                if (hasFinalScore)
                {
                    foreach (var item in studentScores)
                    {
                        var endtermScore = item.Scores.Children.FirstOrDefault(c => c.ColumnName.Equals(Constants.Score.EndtermColumnName));

                        endtermScores.Add(endtermScore!.Value ?? 0);
                    }
                }
                var endtermStatistic = new ColumnStatistics();
                endtermStatistic.Distribution = CalculateScoreDistribution(endtermScores);
                endtermStatistic.Max = endtermScores.Max();
                endtermStatistic.Min = endtermScores.Min();
                endtermStatistic.Average = endtermScores.Average();
                endtermStatistic.ColumnName = Constants.Score.EndtermColumnName;

                statistics.ColumnStatistics.Add(endtermStatistic);

            }


            return statistics;
        }
        private Dictionary<string, int> CalculateScoreDistribution(List<double> scores)
        {
            var distribution = new Dictionary<string, int>
            {
                {"F", 0},
                {"D", 0},
                {"C", 0},
                {"B", 0},
                {"A", 0}
            };
            foreach (var score in scores)
            {
                if (score >= 0 && score < 4) distribution["F"]++;
                else if (score >= 4 && score < 5.4) distribution["D"]++;
                else if (score >= 5.5 && score < 6.9) distribution["C"]++;
                else if (score >= 7 && score < 8.4) distribution["B"]++;
                else if (score >= 8.5 && score <= 10) distribution["A"]++;
            }

            return distribution;
        }
        public ScoreStructure ChangeScoreStructureIdForAllChildren(ScoreStructure scoreStructure,string courseId)
        {
            var children = scoreStructure.Children;
            foreach (var child in children)
            {
                child.Id = Guid.NewGuid().ToString(); 
                child.CourseId = courseId;
                ChangeScoreStructureIdForAllChildren(child, courseId);
            }
            return scoreStructure;
        }
    }
}
    