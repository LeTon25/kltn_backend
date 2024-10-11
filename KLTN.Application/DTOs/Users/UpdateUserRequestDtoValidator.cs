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
                .NotEmpty().WithMessage("SĐT không được để trống");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ tên không được để trống");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Vui lòng chọn giới tính");

            RuleFor(x => x.DoB)
                .NotEmpty().WithMessage("Vui lòng chọn ngày sinh");
        }  

    }
}
