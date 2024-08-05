using KLTN.Application.Interfaces;
using KLTN.Domain.Interfaces;
using KLTN.Infrastructure.Repositories;
using KLTN.Application.Services;
namespace KLTN.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMyService(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IExampleService, ExampleService>();
            return services;
        }
    }
}
