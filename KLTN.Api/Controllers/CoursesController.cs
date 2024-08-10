using AutoMapper;
using KLTN.Application.DTOs.Courses;
using KLTN.Application.Helpers.Filter;
using KLTN.Application.Helpers.Pagination;
using KLTN.Application.Helpers.Response;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KLTN.Api.Controllers
{
    public class CoursesController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public CoursesController(ApplicationDbContext _db,
            IMapper _mapper) 
        { 
            this._db = _db;
            this._mapper = _mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCoursesAsync()
        {
            var query = _db.Courses;

            var courseDtos = await query.ToListAsync();
            return Ok(_mapper.Map<List<CourseDto>>(courseDtos));
        }
        [HttpGet("filter")]
        public async Task<IActionResult> GetCoursesPagingAsync(string filter, int pageIndex, int pageSize)
        {
            var query = _db.Courses.AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(e => e.CourseGroup.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            var data = items.Select(e => _mapper.Map<CourseDto>(e)).ToList();

            var pagination = new Pagination<CourseDto>
            {
                Items = data,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        [HttpGet("{lecturerId}/filter")]
        public async Task<IActionResult> GetByIdAsync(string lecturerId, int pageIndex, int pageSize)
        {
            var query = _db.Courses.AsQueryable();

            query = query.Where(x => x.LecturerId == lecturerId); 

            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            var data = items.Select(e => _mapper.Map<CourseDto>(e)).ToList();

            var pagination = new Pagination<CourseDto>
            {
                Items = data,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostCourseAsync(CreateCourseRequestDto requestDto)
        {
            var courses = _db.Courses;
            var newCourseId = Guid.NewGuid();
            var newCourse = new Course()
            {
                CourseId = newCourseId.ToString(),
                CourseGroup = requestDto.CourseGroup,
                EnableInvite = requestDto.EnableInvite,
                InviteCode = requestDto.InviteCode ?? GenerateRandomNumericString(6),
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
            };
            var result = await _db.AddAsync(newCourse);
            await _db.SaveChangesAsync();
            return Ok(_mapper.Map<CourseDto>(result));
        }
        [HttpPut("{courseId}")]
        [ApiValidationFilter]
        public async Task<IActionResult> PutCourseId(string courseId, [FromBody] CreateCourseRequestDto requestDto)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if(course == null)
            {
                return NotFound(new ApiNotFoundResponse("Không tìm thấy lớp"));
            }    
            course.CourseGroup = requestDto.CourseGroup;
            course.EnableInvite = requestDto.EnableInvite;
            course.InviteCode = requestDto.InviteCode;
            course.UpdatedAt = DateTime.Now;

            _db.Courses.Update(course);
            var result = await _db.SaveChangesAsync();
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse("Cập nhật lớp học thất bại"));
        }

        [HttpDelete("{courseId}")]
        public async Task<IActionResult> DeleteCourseAsync(string courseId)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                return NotFound(new ApiNotFoundResponse("Không thể tìm thấy lớp học với id"));
            }
            _db.Courses.Remove(course);
            var result = await _db.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(_mapper.Map<CourseDto>(course));
            }
            return BadRequest(new ApiBadRequestResponse("Xóa thông tin lớp học thất bại"));
        }
        [HttpPost("{courseId}/apply-code")]
        public async Task<IActionResult> PostApplyCode(string courseId,JoinCourseViaCodeDto requestDto )
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null) { 
                return NotFound(new ApiNotFoundResponse("Không tìm thấy lớp học"));
            }
            
            if(course.EnableInvite == false)
            {
                return BadRequest(new ApiBadRequestResponse("Không thể tham gia lớp học qua mã mời.Vui lòng liên hệ giáo viên"));
            }    

            if(course.InviteCode != requestDto.InviteCode)
            {
                return BadRequest(new ApiBadRequestResponse("Mã lớp học không chính xác"));
            }

            await _db.EnrolledCourse.AddAsync(new EnrolledCourse()
            {
                CourseId = courseId,
                StudentId = "tmp"
            });
            var result = await _db.SaveChangesAsync();
            if (result > 0) {
                return Ok();
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Không thể tham gia lớp học"));
            }
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
