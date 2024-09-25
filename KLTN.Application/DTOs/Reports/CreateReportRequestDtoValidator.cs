using FluentValidation;
using KLTN.Application.DTOs.Announcements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Reports
{
    public class CreateReportRequestDtoValidator : AbstractValidator<CreateReportRequestDto>
    {
        public CreateReportRequestDtoValidator() 
        {
            RuleFor(c=>c.Content).NotEmpty().WithName("Nội dung không được bỏ trống");
        }
    }
}
