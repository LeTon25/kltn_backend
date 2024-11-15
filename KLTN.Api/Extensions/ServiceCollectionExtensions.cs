
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
using KLTN.Infrastructure.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

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

        public static IServiceCollection AddCustomHttpServices(this IServiceCollection services)
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
        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services,IConfiguration configuration)
        {
            var googleOptions = configuration.GetSection(nameof(KLTN.Domain.Settings.GoogleOptions)).Get<KLTN.Domain.Settings.GoogleOptions>();
            if (googleOptions == null || googleOptions.ClientId == null || googleOptions.ClientSecret == null || googleOptions.CallbackPath == null)
                throw new Exception("Missing Google Options");
            services.AddSingleton<GoogleOptions>(googleOptions!);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("JWT:SigningKey")!.Value))
                };
            })
            .AddCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.IsEssential = true;
                options.LoginPath = "/signin-google";
            })
            .AddGoogle(
                options =>
                {
                    options.ClientId = googleOptions!.ClientId;
                    options.ClientSecret = googleOptions.ClientSecret;
                    options.SaveTokens = true;
                    options.Scope.Add("profile");
                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            var picture = context.User.GetProperty("picture").GetString();

                            context.Identity.AddClaim(new Claim("picture", picture));
                        },
                      

                    };
                } 
            );
            return services;
        }
    }
}
