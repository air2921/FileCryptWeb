using data_access.Ef;
using data_access.Redis;
using domain.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace data_access
{
    public static class ServiceRegistration
    {
        public static void AddDataInfrastructure(this IServiceCollection services,
            IConfiguration _config, Serilog.ILogger logger)
        {
            Console.WriteLine($"Redis: {_config.GetConnectionString("Redis")}");
            Console.WriteLine($"PostreSQL: {_config.GetConnectionString("Postgres")}");

            services.AddDbContext<FileCryptDbContext>(options =>
            {
                options.UseNpgsql(_config.GetConnectionString("Postgres"))
                .EnableServiceProviderCaching(false)
                .EnableDetailedErrors(true)
                .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
            });

            services.AddLogging(log =>
            {
                log.ClearProviders();
                log.AddSerilog(logger);
            });

            services.AddScoped<ISeed, FileCryptDbContext>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IDatabaseTransaction, DatabaseTransaction>();
            services.AddScoped<IRedisDbContext>(provider => new RedisDbContext(_config));
            services.AddScoped<IRedisCache, RedisCache>();
        }
    }
}
