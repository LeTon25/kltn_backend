using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Courses
{
    public class ImportStudentValidator : AbstractValidator<ImportStudent>
    {
        public ImportStudentValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên sinh viên không được để trống");
            RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email không được để trống")
                    .Matches(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").WithMessage("Email không đúng định dạng");
            RuleFor(x => x.CustomId)
                .NotEmpty().WithMessage("Mã sinh viên không được để trống");
        }
    }
}
