using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Courses
{
    public class CreateCourseRequestDtoValidator : AbstractValidator<CreateCourseRequestDto>
    {
        public CreateCourseRequestDtoValidator() 
        { 
            RuleFor(c=>c.CourseGroup).NotEmpty().WithMessage("Không được bỏ trống nhóm lớp");
            RuleFor(c => c.Name).NotEmpty().WithMessage("Không bỏ trống tên lớp học");
        }
    }
}
