using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Subjects
{
    public class CreateSubjectRequestDtoValidator : AbstractValidator<CreateSubjectRequestDto>
    {
        public CreateSubjectRequestDtoValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên môn học không được để trống")
                .MaximumLength(100).WithMessage("Tên môn học có tối đa 100 kí tự");
        }
    }
}
