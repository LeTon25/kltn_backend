
using KLTN.Api.Filters;
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
            services.AddTransient<ITokenService, TokenService>();  

            //
            services.AddTransient<IUnitOfWork,UnitOfWork>();
            services.AddTransient<SubjectService>();
            services.AddTransient<CommentService>();
            services.AddTransient<AnnoucementService>();
            services.AddTransient<CourseService>();
            services.AddTransient<AccountService>();
            services.AddTransient<ProjectService>();
            services.AddTransient<GroupService>();
            services.AddTransient<AssignmentService>();
            services.AddTransient<ScoreStructureService>();
            services.AddTransient<ReportService>(); 
            services.AddTransient<SubmissionService>(); 
            services.AddTransient<ScoreServices>();
            services.AddTransient<BriefService>();
            //
            services.AddScoped<CourseResourceAccessFilter>();
            services.AddScoped<GroupResourceAccessFilter>();
            return services;
        }
    }
}
