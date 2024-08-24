using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.Util;
using System.Net.WebSockets;

namespace KLTN.Api.Extensions
{
    public static class AwsExtension
    {
        public static IServiceCollection AddAwsStorageService(this IServiceCollection services,IConfiguration configuration)
        {
            var awsOptions= configuration.GetAWSOptions("AWS");
            services.AddDefaultAWSOptions(awsOptions);  
            services.AddAWSService<IAmazonS3>();
            return services;
        }

    }
}
