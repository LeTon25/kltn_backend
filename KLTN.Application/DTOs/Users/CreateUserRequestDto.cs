using KLTN.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KLTN.Application.DTOs.Users
{
    [ValidateNever]
    public class CreateUserRequestDto
    {
        
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public DateTime? DoB { get; set; }
        public string Gender { get; set; }
        public string? CustomId { get; set; }
        public UserType UserType { get; set; }
        public IFormFile? File { get; set; }
    }
}
