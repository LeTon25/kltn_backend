using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace KLTN.Application.DTOs.Users
{
    [ValidateNever]
    public class UpdateUserRequestDto
    {
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public DateTime? DoB { get; set; }
        public string Gender { get; set; }
        public string? Avatar { get; set; }
    }
}
