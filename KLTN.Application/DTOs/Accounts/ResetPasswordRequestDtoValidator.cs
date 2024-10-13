using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Accounts
{
    public class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
    {
        public ResetPasswordRequestDtoValidator() 
        {
            RuleFor(x => x.Token).NotEmpty().WithMessage("Token không được bỏ trống");
            RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Mật khẩu không được để trống")
                    .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 kí tự")
                    .Matches(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$")
                    .WithMessage("Mật khẩu có chứa kí tự thường,in hoa,số,các kí tự đặc biệt(#?!@$%^&*-)");
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Mật khẩu nhập lại không chính xác");
            RuleFor(x => x.Email).EmailAddress().WithMessage("Email không đúng định dạng");

        }
    }
}
