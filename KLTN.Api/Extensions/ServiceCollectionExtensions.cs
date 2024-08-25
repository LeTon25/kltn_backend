
using KLTN.Api.Services.Implements;
using KLTN.Api.Services.Interfaces;
using KLTN.Application;
using KLTN.Application.Services;
using KLTN.Domain.Repositories;
using KLTN.Infrastructure.Repositories;
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

            //
            services.AddScoped<IUnitOfWork,UnitOfWork>();
            services.AddScoped<SubjectService>();
            services.AddScoped<CommentService>();
            services.AddScoped<AnnoucementService>();
            services.AddScoped<CourseService>();
            services.AddScoped<AccountService>();

            return services;
        }
    }
}
