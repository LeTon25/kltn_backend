using FluentValidation;

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
        }   
    }
}
