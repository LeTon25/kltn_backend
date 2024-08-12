using AutoMapper;
using KLTN.Application.DTOs.Semesters;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.Net.WebSockets;

namespace KLTN.Api.Controllers
{
    public class SemestersController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public SemestersController(ApplicationDbContext db,IMapper mapper) {
            this._db = db;
            this._mapper = mapper;  
        }
        [HttpGet]
        public async Task<IActionResult> GetSemestersAsync()
        {
            var query = _db.Semesters;

            var semesterDtos = await query.ToListAsync();
            return Ok(_mapper.Map<List<SemesterDto>>(semesterDtos));
        }
        [HttpGet("filter")]
        public async Task<IActionResult> GetSemestersPagingAsync(string filter,int pageIndex,int pageSize)
        {
            var query = _db.Semesters.AsQueryable();
            if(!string.IsNullOrEmpty(filter))
            {
                query = query.Where(e => e.Name.Contains(filter));
            }    
            var totalRecords = await query.CountAsync();
            var items= await query.Skip((pageIndex-1)*pageSize).Take(pageSize).ToListAsync();
            var data = items.Select(e => _mapper.Map<SemesterDto>(e)).ToList();

            var pagination = new Pagination<SemesterDto>
            {
                Items = data,
                TotalRecords = totalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(pagination);
        }
        [HttpGet("{semesterId}")]
        public async Task<IActionResult> GetByIdAsync(string semesterId)  
        {
            var semester = await _db.Semesters.FindAsync(semesterId);
            if(semester == null)
            {
                return NotFound(new ApiNotFoundResponse("Không tìm thấy học kỳ cần tìm"));
            }
            return Ok(_mapper.Map<SemesterDto>(semester));  

        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostSemesterAsync(CreateSemesterRequestDto requestDto)
        {
            var semesters = _db.Semesters;
            if (await semesters.AnyAsync(c => c.Name.Equals(requestDto.Name))) 
            {
                return BadRequest(new ApiBadRequestResponse("Tên học kì không được trùng"));
            }
            if (requestDto.StartDate > requestDto.EndDate) 
            {
                return BadRequest(new ApiBadRequestResponse("Ngày bắt đầu không được lớn hơn ngày kết thúc"));
            }
            var newSemesterId = Guid.NewGuid();
            var newSemester = new Semester()
            {
                SemesterId = newSemesterId.ToString(),
                Name = requestDto.Name,
                StartDate = requestDto.StartDate,
                EndDate = requestDto.EndDate,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
            };
            var result = await _db.AddAsync(newSemester);
            await _db.SaveChangesAsync();
            return Ok(_mapper.Map<SemesterDto>(newSemester));
        }
        [HttpPut("{semesterId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutSemesterId(string semesterId, [FromBody] CreateSemesterRequestDto requestDto)
        {
            var semester = await _db.Semesters.FirstOrDefaultAsync(c=>c.SemesterId == semesterId);
            if (semester == null) {
                return NotFound(new ApiNotFoundResponse($"Không tìm thấy học kì với id : {semesterId}"));
            }
            if(await _db.Semesters.AnyAsync(e=>e.Name == requestDto.Name && e.SemesterId != semesterId))
            {
                return BadRequest(new ApiBadRequestResponse("Tên học kì không được trùng"));
            }
            if (requestDto.StartDate > requestDto.EndDate)
            {
                return BadRequest(new ApiBadRequestResponse("Ngày bắt đầu không được lớn hơn ngày kết thúc"));
            }
            semester.Name = requestDto.Name;
            semester.StartDate = requestDto.StartDate;
            semester.EndDate = requestDto.EndDate;
            semester.UpdatedAt = DateTime.Now;

            _db.Semesters.Update(semester);
            var result = await _db.SaveChangesAsync();
            if(result > 0)
            {
                return NoContent(); 
            }
            return BadRequest(new ApiBadRequestResponse("Cập nhật học kì thất bại"));
        }

        [HttpDelete("{semesterId}")]
        public async Task<IActionResult> DeleteSemesterAsync(string semesterId)
        {
            var semester = await _db.Semesters.FirstOrDefaultAsync(c => c.SemesterId == semesterId);
            if (semester == null) {
                return NotFound(new ApiNotFoundResponse("Không thể tìm thấy học kì với id"));
            }
            _db.Semesters.Remove(semester);
            var result = await _db.SaveChangesAsync();
            if (result > 0) {
                return Ok(_mapper.Map<SemesterDto>(semester));
            }
            return BadRequest(new ApiBadRequestResponse("Xóa thông tin học kì thất bại"));
        }

    }
}
