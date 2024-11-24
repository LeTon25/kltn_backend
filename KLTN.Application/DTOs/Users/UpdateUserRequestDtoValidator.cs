using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Users
{
    public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
    {
        public UpdateUserRequestDtoValidator() 
        {
            RuleFor(x => x.PhoneNumber)
                .NotNull().WithMessage("SĐT không được để trống");

            RuleFor(x => x.FullName)
                .NotNull().WithMessage("Họ tên không được để trống");

            RuleFor(x => x.Gender)
                .NotNull().WithMessage("Vui lòng chọn giới tính");

            RuleFor(x => x.DoB)
                .NotNull().WithMessage("Vui lòng chọn ngày sinh");
        }  

    }
    public class UpdateUserByAdminRequestDtoValidator : AbstractValidator<UpdateUserByAdminRequestDto>
    {
        public UpdateUserByAdminRequestDtoValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotNull().WithMessage("SĐT không được để trống");

            RuleFor(x => x.FullName)
                .NotNull().WithMessage("Họ tên không được để trống");

            RuleFor(x => x.Gender)
                .NotNull().WithMessage("Vui lòng chọn giới tính");

            RuleFor(x => x.DoB)
                .NotNull().WithMessage("Vui lòng chọn ngày sinh");

            RuleFor(x => x.CustomId)
                .NotNull().WithMessage("Vui lòng nhập mã cho người dùng");
        }

    }
}
