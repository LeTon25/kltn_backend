
using KLTN.Api.Services.Implements;
using KLTN.Api.Services.Interfaces;
using KLTN.Application;
using KLTN.Infrastructure.Seeders;

namespace KLTN.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMyService(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddTransient<IStorageService, FileLocalStorageService>();
            services.AddTransient<DbInitializer>();
            services.AddScoped<ITokenService, TokenService>();  
            return services;
        }
    }
}
