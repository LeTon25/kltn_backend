using Hangfire;
using KLTN.Domain.Settings;

namespace KLTN.BackgroundJobs.Extensions
{
    public static class HostExtension
    {
        public static void AddAppConfigurations(this WebApplicationBuilder builder)
        {
            var env = builder.Environment;
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true,
                        reloadOnChange: true)
                    .AddEnvironmentVariables();
        }
        public static IApplicationBuilder UseCustomHangfireDashboard(this IApplicationBuilder app,IConfiguration configuration) 
        {
            var configDashboard = configuration.GetSection("HangFireSettings:Dashboard").Get<DashboardOptions>();
            var hangfireSettings = configuration.GetSection("HangFireSettings").Get<HangFireSettings>();

            if (configDashboard == null)
                throw new Exception("Hangfire Settings => Dashboard is missing");
            if (hangfireSettings == null)
                throw new Exception("Hangfire Settings is missing");
            app.UseHangfireDashboard(hangfireSettings.Route, new DashboardOptions
            {
                //Authorization = new object[] {},
                DashboardTitle = configDashboard.DashboardTitle,
                StatsPollingInterval = configDashboard.StatsPollingInterval,
                AppPath = configDashboard.AppPath,
                IgnoreAntiforgeryToken = true
            });
            return app;
        }
    }
}
