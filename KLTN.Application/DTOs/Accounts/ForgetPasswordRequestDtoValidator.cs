using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Accounts
{
    public class ForgetPasswordRequestDtoValidator : AbstractValidator<ForgetPasswordRequestDto>
    {
        public ForgetPasswordRequestDtoValidator() 
        {
            RuleFor(c => c.Email).EmailAddress().WithMessage("Email không đúng định dạng");
        }  
    }
}
