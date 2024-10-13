using KLTN.Api.Services.Implements;
using KLTN.Api.Services.Interfaces;
using KLTN.Application;
using KLTN.Domain.Entities;
using KLTN.Infrastructure.Data;
using KLTN.Infrastructure.Seeders;
using Microsoft.AspNetCore.Identity;

namespace KLTN.Api.Extensions
{
    public static class ConfigureIndentityExtension
    {
        public static IServiceCollection ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<User,IdentityRole>(options =>
            {
                PasswordOptions passwordOptions = GetPasswordOptions();
                options.Password = passwordOptions;
            })
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddErrorDescriber<CustomIdentityErrorDescriber>();  
            return services;
        }
        private static PasswordOptions GetPasswordOptions()
        {
            return new PasswordOptions
            {
                RequiredLength = 8,
                RequireNonAlphanumeric = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireDigit = true,
                RequiredUniqueChars = 1
            };
        }
    }
}
