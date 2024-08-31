using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Accounts
{
    public class GeneratePasswordRequestDtoValidator : AbstractValidator<GeneratePasswordRequestDto>
    {
        public GeneratePasswordRequestDtoValidator()
        {
            RuleFor(c=>c.Email)
                   .NotEmpty()
                   .WithMessage("Email không được bỏ trống"); 
        }
    }
}
