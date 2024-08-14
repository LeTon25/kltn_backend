using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Accounts
{
    public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestDtoValidator() 
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username không được để trống");
            RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Mật khẩu không được để trống")
                    .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 kí tự")
                    .Matches(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$")
                    .WithMessage("Mật khẩu có chứa kí tự thường,in hoa,số,các kí tự đặc biệt(#?!@$%^&*-)");
        }
    }
}
