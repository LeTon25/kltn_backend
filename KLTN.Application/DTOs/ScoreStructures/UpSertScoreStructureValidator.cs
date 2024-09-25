using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.ScoreStructures
{
    public class UpSertScoreStructureValidator : AbstractValidator<UpSertScoreStructureDto>
    {
        public UpSertScoreStructureValidator() 
        {
            RuleFor(c => c.ColumnName).NotEmpty().WithMessage("Không được bỏ trống tên cột");
            
            RuleFor(c => c.Percent)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Phần trăm phải từ 1 - 100");

            RuleFor(c => c.MaxPercent)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Phần trăm phải từ 1 - 100");
        }   
    }
}
