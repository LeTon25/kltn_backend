using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Semesters
{
    public class CreateSemesterRequestDtoValidator : AbstractValidator<CreateSemesterRequestDto>
    {
        public CreateSemesterRequestDtoValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên học kì không được để trống")
                .MaximumLength(100).WithMessage("Tên học kì có tối đa 100 kí tự");
        }
    }
}
