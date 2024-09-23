using KLTN.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KLTN.Application.Helpers.Response;
using KLTN.Application.DTOs.Subjects;
using AutoMapper;
using KLTN.Domain.Entities;
namespace KLTN.Application.Services
{
    public class SubjectService
    {
        private readonly IUnitOfWork _unitOfWork;   
        private readonly IMapper mapper;
        public SubjectService(IUnitOfWork unitOfWork, IMapper _mapper) 
        {
            this._unitOfWork = unitOfWork;
            this.mapper = _mapper;
        }
        public async Task<ApiResponse<List<SubjectDto>>> GetAllSubjectAsync()
        {
            var data =  await _unitOfWork.SubjectRepository.GetAllAsync();

            return new ApiResponse<List<SubjectDto>>(200, "Thông báo", mapper.Map<List<SubjectDto>>(data.ToList())); 
        }
        public async Task<ApiResponse<object>> GetByIdAsync(string Id)
        {
            var data = await _unitOfWork.SubjectRepository.GetFirstOrDefaultAsync(x => x.SubjectId == Id);
            if(data == null)
            {
                return new ApiNotFoundResponse<object>("Không tìm thấy học kỳ cần tìm");
            }
            return new ApiResponse<object>(200, "Thành công", mapper.Map<SubjectDto>(data));
        }
        public async Task<ApiResponse<object>> AddSubjectAsync(CreateSubjectRequestDto requestDto)
        {
            if (await _unitOfWork.SubjectRepository.AnyAsync(c => c.Name.Equals(requestDto.Name)))
            {
                return new ApiBadRequestResponse<object>("Tên môn học không được trùng");
            }
            if (await _unitOfWork.SubjectRepository.AnyAsync(c => c.SubjectCode.Equals(requestDto.SubjectCode)))
            {
                return new ApiBadRequestResponse<object>("Mã môn học không được trùng");
            }
            var newSubjectId = Guid.NewGuid();
            var newSubject = new Subject()
            {
                SubjectId = newSubjectId.ToString(),
                Name = requestDto.Name,
                SubjectCode = requestDto.SubjectCode,
                Description = requestDto.Description,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
            };
             await _unitOfWork.SubjectRepository.AddAsync(newSubject);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<object>(200, "Thành công", mapper.Map<SubjectDto>(newSubject));
        }
        public async Task<ApiResponse<object>> DeleteSubjectAsync(string subjectId)
        {
            var subject = await _unitOfWork.SubjectRepository.GetFirstOrDefaultAsync(c => c.SubjectId == subjectId);
            if (subject == null)
            {
                return new ApiNotFoundResponse<object>("Không thể tìm thấy môn học với id");
            }
            _unitOfWork.SubjectRepository.Delete(subject);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Thành công", mapper.Map<SubjectDto>(subject));
            }
            return new ApiBadRequestResponse<object>("Xóa thông tin môn học thất bại");
        }
        public async Task<ApiResponse<object>> UpdateSubjectAsync(string subjectId, CreateSubjectRequestDto requestDto)
        {
            var subject = await _unitOfWork.SubjectRepository.GetFirstOrDefaultAsync(c => c.SubjectId == subjectId);
            if (subject == null)
            {
                return new ApiNotFoundResponse<object>($"Không tìm thấy môn học với id : {subjectId}");
            }
            if (await _unitOfWork.SubjectRepository.AnyAsync(e => e.Name == requestDto.Name && e.SubjectId != subjectId))
            {
                return new ApiBadRequestResponse<object>("Tên môn học không được trùng");
            }

            if (await _unitOfWork.SubjectRepository.AnyAsync(e => e.SubjectCode == requestDto.SubjectCode && e.SubjectId != subjectId))
            {
                return new ApiBadRequestResponse<object>("Mã môn học không được trùng");
            }
            subject.Name = requestDto.Name;
            subject.Description = requestDto.Description;
            subject.SubjectCode = requestDto.SubjectCode ?? subject.SubjectCode;

            _unitOfWork.SubjectRepository.Update(subject);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                return new ApiResponse<object>(200, "Cập nhật thành công", mapper.Map<SubjectDto>(subject));

            }
            return new ApiBadRequestResponse<object>("Cập nhật môn học thất bại");
        }

    }
}
