
using KLTN.Api.Filters;
using KLTN.Api.Services.Implements;
using KLTN.Api.Services.Interfaces;
using KLTN.Application;
using KLTN.Application.Services;
using KLTN.Application.Services.HttpServices;
using KLTN.Domain.Repositories;
using KLTN.Domain.Services;
using KLTN.Domain.Settings;
using KLTN.Infrastructure.Mailing;
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
            services.AddTransient<RequestService>();
            services.AddTransient<SettingService>();
            //
            services.AddScoped<CourseResourceAccessFilter>();
            services.AddScoped<GroupResourceAccessFilter>();
            return services;
        }

        public static IServiceCollection AddCustomHttpServices(this ServiceCollection services)
        {
            services.AddHttpClient<BackgroundJobHttpService>();
            return services;
        }
        public static IServiceCollection ConfigureEmailSettings(this IServiceCollection services,IConfiguration configuration)
        {
            var emailSettings = configuration.GetSection(nameof(SMTPEmailSettings))
                .Get<SMTPEmailSettings>();
            services.AddSingleton<ISMTPEmailSettings>(emailSettings!);
            services.AddTransient<ISMTPEmailService, SmtpEmailService>();

            return services;
        }
    }
}
