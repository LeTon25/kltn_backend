using AutoMapper;
using KLTN.Application.DTOs.ScoreStructures;
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
        public async Task<ApiResponse<object>> CreateScoreStructureAsync(CreateScoreStuctureRequestDto requestDto)
        {
            //Kiểm tra điều kiện
            if (!string.IsNullOrEmpty(requestDto.ParentId))
            {
                var parentScoreStructure = await GetScoreStructureDtoAsync(requestDto.ParentId);
                if (parentScoreStructure == null)
                {
                    return new ApiNotFoundResponse<object>("Cột điểm cha không tồn tại");
                }
                // Tổng trọng số của các tiêu chí con không được vượt quá 100%
                var totalSubScoreStructurePercent = parentScoreStructure.Childrens.Sum(c => c.Percent);
                if (totalSubScoreStructurePercent + requestDto.Percent > 100)
                {
                    return new ApiBadRequestResponse<object>("Tổng trọng số của các tiêu chí con không được vượt quá 100%.");
                }
            }
            var entity = _mapper.Map<ScoreStructure>(requestDto);

            var newId = Guid.NewGuid().ToString();
            entity.Id = newId;
            await _unitOfWork.ScoreStructureRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var response = await GetScoreStructureDtoAsync(newId);
            return new ApiResponse<object>(200,"Tạo cột điểm thành công",response);
        }
        public async Task<ApiResponse<object>> DeleteScoreStructureAsync(string id)
        {
            var scoreStructure = await _unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c => c.Id.Equals(id));
            if(scoreStructure == null)
            {
                return new ApiResponse<object>(404, "Không tìm thấy cột điểm");
            }
            _unitOfWork.ScoreStructureRepository.Delete(scoreStructure);
            var result = await _unitOfWork.SaveChangesAsync();  
            if(result > 0)
            {
                return new ApiBadRequestResponse<object>("Xóa thành công");
            }
            return new ApiBadRequestResponse<object>("Xóa thông tin cột điểm thất bại");

        }
        public async Task<ApiResponse<object>> UpdateScoreStructureAsync(string id,CreateScoreStuctureRequestDto requestDto)
        {
            var scoreStructure = await _unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c => c.Id.Equals(id));
            if (scoreStructure == null)
            {
                return new ApiResponse<object>(404, "Không tìm thấy cột điểm");
            }
            scoreStructure.ParentId = requestDto.ParentId;
            scoreStructure.ColumnName = requestDto.ColumnName;
            scoreStructure.MaxScore = requestDto.MaxScore;
            scoreStructure.Percent = requestDto.Percent;
            //Kiểm tra điều kiện
            if (!string.IsNullOrEmpty(scoreStructure.ParentId))
            {
                var parentScoreStructure = await GetScoreStructureDtoAsync(requestDto.ParentId);
                if (parentScoreStructure == null)
                {
                    return new ApiNotFoundResponse<object>("Cột điểm cha không tồn tại");
                }
                // Tổng trọng số của các tiêu chí con không được vượt quá 100%
                var totalSubScoreStructurePercent = parentScoreStructure.Childrens.Where(c=>!c.Id.Equals(id)).Sum(c => c.Percent);
                if (totalSubScoreStructurePercent + requestDto.Percent > 100)
                {
                    return new ApiBadRequestResponse<object>("Tổng trọng số của các tiêu chí con không được vượt quá 100%.");
                }
            }
            _unitOfWork.ScoreStructureRepository.Update(scoreStructure);
            var result = await _unitOfWork.SaveChangesAsync();
            if(result > 0)
            {
                var response = await GetScoreStructureDtoAsync(id);
                return new ApiResponse<object>(200, "Cập nhật cột điểm thành công", response);
            }
            return new ApiBadRequestResponse<object>("Cập nhật cột điểm thất bại");

        }
        public async Task<ApiResponse<object>> GetScoreStructureAsync(string id)
        {
            var data = await GetScoreStructureDtoAsync(id);
            if(data == null)
            {
                return new ApiNotFoundResponse<object>("Cột điểm không tồn tại"); 
            }
            return new ApiSuccessResponse<object>(200, "Lấy dữ liệu thành công", data);
        }
        #endregion

        #region for_service
        public async Task<ScoreStructureDto> GetScoreStructureDtoAsync(string id)
        {
            var data = await _unitOfWork.ScoreStructureRepository.GetFirstOrDefaultAsync(c => c.Id.Equals(id), false, c => c.Parent, c => c.Children);
            if(data == null)
            {
                return null;
            }
            var dto = _mapper.Map<ScoreStructureDto>(data); 
            return dto;
        }
        public async Task<ScoreStructureDto> GetFullScoreStructureTreeAsync(string id)
        {
            var data = await GetScoreStructureDtoAsync(id);
            if(data == null)
            {
                return null;
            }
            foreach (var item in data.Childrens)
            {
                item.Childrens = await GetChildrenAsync(item.Id);
            }
            return data;
        }
        public async Task<List<ScoreStructureDto>> GetChildrenAsync(string id)
        {
            var subScoreStructure = await GetScoreStructureDtoAsync(id);
            foreach(var item in subScoreStructure.Childrens)
            {
                item.Childrens = await GetChildrenAsync(item.Id);
            }
            return subScoreStructure.Childrens.ToList();
        }
        #endregion
    }
}
