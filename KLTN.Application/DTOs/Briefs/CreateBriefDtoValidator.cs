using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Briefs
{
    public class CreateBriefDtoValidator : AbstractValidator<CreateBriefDto>    
    {
        public CreateBriefDtoValidator() 
        {
            RuleFor(c => c.Content).NotEmpty().WithMessage("Nội dung tóm tắt đang trống");
        }
    }
}
