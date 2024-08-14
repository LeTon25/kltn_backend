using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Projects
{
    public class CreateProjectRequestDtoValidator : AbstractValidator<CreateProjectRequestDto>  
    {
        public CreateProjectRequestDtoValidator() 
        {
            RuleFor(c=>c.Title).NotEmpty().WithMessage("Không được để trống tên đề tài");
            RuleFor(c => c.Description).NotEmpty().WithMessage("Không được để trống mô tả đề tài");
        }
    }
}
