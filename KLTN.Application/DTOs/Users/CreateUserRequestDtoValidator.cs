using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Users
{
    public class CreateUserRequestDtoValidator : AbstractValidator<CreateUserRequestDto>
    {
        public CreateUserRequestDtoValidator() 
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username không được để trống");

            RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Mật khẩu không được để trống")
                    .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 4 kí tự")
                    .Matches(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$")
                    .WithMessage("Mật khẩu có chứa kí tự thường,in hoa,số,các kí tự đặc biệt(#?!@$%^&*-)");

            RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email không được để trống")
                    .Matches(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").WithMessage("Email không đúng định dạng");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("SĐT không được để trống");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ tên không được để trống");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Vui lòng chọn giới tính");
            RuleFor(x => x.DoB)
                .NotEmpty().WithMessage("Vui lòng chọn ngày sinh");
            RuleFor(x => x.UserType)
                .NotEmpty().WithMessage("Vui lòng chọn loại người dùng");
        }  

    }
}
