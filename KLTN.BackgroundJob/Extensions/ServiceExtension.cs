using Hangfire;
using Hangfire.MySql;
using KLTN.BackgroundJobs.ScheduleJobs;
using KLTN.BackgroundJobs.Services;
using KLTN.BackgroundJobs.Services.Interfaces;
using KLTN.Domain.ScheduleJobs;
using KLTN.Domain.Services;
using KLTN.Domain.Settings;
using KLTN.Infrastructure.Mailing;

namespace KLTN.BackgroundJobs.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddCustomService(this IServiceCollection services)
        {
            services.AddTransient<IScheduleJobService,HangfireService>();
            services.AddTransient<IBackgroundJobService, BackgroundJobService>();
            return services;
        }
        public static IServiceCollection AddHangfireService(this IServiceCollection services,IConfiguration configuration)
        {
            var settingsSection  = configuration.GetSection("HangFireSettings");
            
            var settings = new HangFireSettings(); 
                settingsSection.Bind(settings);

            if (settings == null || settings.Storage == null ||
                string.IsNullOrEmpty(settings.Storage.ConnectionString))
                throw new Exception("HangFireSettings is not configured properly!");

            services.ConfigureHangfireServices(settings);
          
            services.AddHangfireServer(serverOptions
               => { 
                     serverOptions.ServerName = settings.ServerName; 
                     
                  });
            return services;
        }
        public static IServiceCollection ConfigureEmailSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var emailSettings = configuration.GetSection(nameof(SMTPEmailSettings))
                .Get<SMTPEmailSettings>();
            services.AddSingleton<ISMTPEmailSettings>(emailSettings!);
            services.AddTransient<ISMTPEmailService, SmtpEmailService>();
            return services;
        }
        private static IServiceCollection ConfigureHangfireServices(this IServiceCollection services,
            HangFireSettings settings)
        {
            if (string.IsNullOrEmpty(settings.Storage.DBProvider))
                throw new Exception("HangFire DBProvider is not configured.");
            var connectionString = settings.Storage.ConnectionString;
            switch (settings.Storage.DBProvider.ToLower())
            {
                case "mysql":
                    services.AddHangfire(cfg =>
                    {
                    cfg.UseStorage(
                        new MySqlStorage(connectionString,new MySqlStorageOptions
                            {
                            TablesPrefix = "Hangfire_",  
                            TransactionIsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,  
                            QueuePollInterval = TimeSpan.FromSeconds(30),  
                            JobExpirationCheckInterval = TimeSpan.FromHours(1),  
                            CountersAggregateInterval = TimeSpan.FromMinutes(5),
                            PrepareSchemaIfNecessary = true,  
                            DashboardJobListLimit = 5000,
                            TransactionTimeout = TimeSpan.FromMinutes(1)
                        })
                        );
                    });
                    break;
                default:
                    throw new Exception($"HangFire Storage Provider {settings.Storage.DBProvider} is not supported.");
            }

            return services;
        }
    }
}
