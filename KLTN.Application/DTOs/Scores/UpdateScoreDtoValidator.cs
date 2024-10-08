using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Scores
{
    public class UpdateScoreDtoValidator : AbstractValidator<UpdateScoreDto>
    {
        public UpdateScoreDtoValidator() 
        {
            RuleFor(c => c.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Điểm phải từ 0 - 10")
                .LessThanOrEqualTo(10).WithMessage("Điểm phải từ 0 - 10");
        }
    }
}
