using FluentValidation;
namespace KLTN.Application.DTOs.Roles
{
    public class CreateRoleRequestDtoValidator : AbstractValidator<CreateRoleRequestDto>
    {
        public CreateRoleRequestDtoValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên vai trò không được để trống")
                .MaximumLength(50).WithMessage("Tên vai trò có tối đa 50 kí tự");
        }
    }
}
