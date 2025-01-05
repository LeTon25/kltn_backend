using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Courses
{
    public class CreateCourseFromTemplateDtoValidator : AbstractValidator<CreateCourseFromTemplateDto>
    {
        public CreateCourseFromTemplateDtoValidator() 
        { 
            RuleFor(c=>c.CourseGroup).NotEmpty().WithMessage("Không được bỏ trống nhóm lớp");
            RuleFor(c => c.SourceCourseId).NotEmpty().WithMessage("Vui lòng chọn lớp mẫu");
            RuleFor(c => c.Name).NotEmpty().WithMessage("Không bỏ trống tên lớp học");
            RuleFor(c => c.Semester).NotEmpty().WithMessage("Không bỏ trống học kỳ");

        }
    }
}
