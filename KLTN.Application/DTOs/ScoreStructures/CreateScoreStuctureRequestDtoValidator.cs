using FluentValidation;
using KLTN.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.ScoreStructures
{
    public class CreateScoreStuctureRequestDtoValidator : AbstractValidator<ScoreStructure>
    {
        public CreateScoreStuctureRequestDtoValidator() {
            RuleFor(c => c.ColumnName)
                .NotEmpty().WithMessage("Tên cột điểm không được để trống");
            RuleFor(c => c.MaxScore)
                .GreaterThan(0).WithMessage("Điểm tối đa phải lớn hơn không");
            RuleFor(c => c.Percent)
                .GreaterThanOrEqualTo(0).WithMessage("Phần trăm điểm phải lớn hơn 0");
            RuleFor(c => c.Percent)
                .NotEmpty().WithMessage("Trọng số cột điểm không được để trống")
                .GreaterThanOrEqualTo(1).WithMessage("Trọng số phải từ 1 - 100")
                .LessThanOrEqualTo(100).WithMessage("Trọng số phải từ 1 - 100");
        } 
    }
}
