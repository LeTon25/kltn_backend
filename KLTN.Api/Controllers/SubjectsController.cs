using AutoMapper;
using KLTN.Application.DTOs.Subjects;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Api.Controllers
{
    public class SubjectsController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public SubjectsController(ApplicationDbContext db,IMapper mapper) {
            this._db = db;
            this._mapper = mapper;  
        }
        [HttpGet]
        public async Task<IActionResult> GetSubjectsAsync()
        {
            var query = _db.Subjects;

            var subjectDtos = await query.ToListAsync();
            return Ok(_mapper.Map<List<SubjectDto>>(subjectDtos));
        }
        [HttpGet("filter")]
        public async Task<IActionResult> GetSubjectsPagingAsync(string filter,int pageIndex,int pageSize)
        {
            var query = _db.Subjects.AsQueryable();
            if(!string.IsNullOrEmpty(filter))
            {
                query = query.Where(e => e.Name.Contains(filter));
            }    
            var totalRecords = await query.CountAsync();
            var items= await query.Skip((pageIndex-1)*pageSize).Take(pageSize).ToListAsync();
            var data = items.Select(e => _mapper.Map<SubjectDto>(e)).ToList();

            var pagination = new Pagination<SubjectDto>
            {
                Items = data,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }
        [HttpGet("{subjectId}")]
        public async Task<IActionResult> GetByIdAsync(string subjectId)
        {
            var subject = await _db.Subjects.FindAsync(subjectId);
            if(subject == null)
            {
                return NotFound(new ApiNotFoundResponse("Không tìm thấy học kỳ cần tìm"));
            }
            return Ok(_mapper.Map<SubjectDto>(subject));  

        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostSubjectAsync(CreateSubjectRequestDto requestDto)
        {
            var subjects = _db.Subjects;
            if (await subjects.AnyAsync(c => c.Name.Equals(requestDto.Name))) 
            {
                return BadRequest(new ApiBadRequestResponse("Tên môn học không được trùng"));
            }
            var newSubjectId = Guid.NewGuid();
            var newSubject = new Subject()
            {
                SubjectId = newSubjectId.ToString(),
                Name = requestDto.Name,
                Description = requestDto.Description,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
            };
            var result = await _db.AddAsync(newSubject);
            await _db.SaveChangesAsync();
            return Ok(_mapper.Map<SubjectDto>(result));
        }
        [HttpPut("{subjectId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutSubjectId(string subjectId, [FromBody] CreateSubjectRequestDto requestDto)
        {
            var subject = await _db.Subjects.FirstOrDefaultAsync(c=>c.SubjectId == subjectId);
            if (subject == null) {
                return NotFound(new ApiNotFoundResponse($"Không tìm thấy môn học với id : {subjectId}"));
            }
            if(await _db.Subjects.AnyAsync(e=>e.Name == requestDto.Name && e.SubjectId != subjectId))
            {
                return BadRequest(new ApiBadRequestResponse("Tên môn học không được trùng"));
            }
 
            subject.Name = requestDto.Name;
            subject.Description  = requestDto.Description;

            _db.Subjects.Update(subject);
            var result = await _db.SaveChangesAsync();
            if(result > 0)
            {
                return NoContent(); 
            }
            return BadRequest(new ApiBadRequestResponse("Cập nhật môn học thất bại"));
        }

        [HttpDelete("{subjectId}")]
        public async Task<IActionResult> DeleteSubjectAsync(string subjectId)
        {
            var subject = await _db.Subjects.FirstOrDefaultAsync(c => c.SubjectId == subjectId);
            if (subject == null) {
                return NotFound(new ApiNotFoundResponse("Không thể tìm thấy môn học với id"));
            }
            _db.Subjects.Remove(subject);
            var result = await _db.SaveChangesAsync();
            if (result > 0) {
                return Ok(_mapper.Map<SubjectDto>(subject));
            }
            return BadRequest(new ApiBadRequestResponse("Xóa thông tin môn học thất bại"));
        }

    }
}
