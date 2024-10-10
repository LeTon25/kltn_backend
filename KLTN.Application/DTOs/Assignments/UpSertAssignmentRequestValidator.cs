using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.DTOs.Assignments
{
    public class UpSertAssignmentRequestValidator : AbstractValidator<UpSertAssignmentRequestDto>
    {
        public UpSertAssignmentRequestValidator() {
            RuleFor(c => c.Title).NotEmpty().WithName("Tiêu đề không được để trống");
        }
    }
}
