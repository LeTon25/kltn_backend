using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Courses
{
    public class JoinCourseViaCodeDtoValidator : AbstractValidator<JoinCourseViaCodeDto>
    {
        public JoinCourseViaCodeDtoValidator() 
        {
            RuleFor(c => c.InviteCode).NotEmpty().WithMessage("Vui lòng nhập mã mời");
        }
    }
}
