using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace additional
{
    public static class ServiceRegistration
    {
        public static void AddAdditionalInfrastructure(this IServiceCollection services, Serilog.ILogger logger)
        {
            services.AddSingleton<IRequest, Request>();
            services.AddSingleton(typeof(ILogger<>), typeof(AdditionalLogger<>));

            services.AddLogging(log =>
            {
                log.ClearProviders();
                log.AddSerilog(logger);
            });
        }
    }
}
