using data_access.Ef;
using data_access.Redis;
using domain.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace data_access
{
    public static class ServiceRegistration
    {
        public static void AddDataInfrastructure(this IServiceCollection services, IConfiguration _config)
        {
            Console.WriteLine($"Redis: {_config.GetConnectionString("Redis")}");
            Console.WriteLine($"PostreSQL: {_config.GetConnectionString("Postgres")}");

            services.Configure<RedisDbContext>(options =>
            {
                options.ConnectionString = _config.GetConnectionString("Redis")!;
            });

            services.AddDbContext<FileCryptDbContext>(options =>
            {
                options.UseNpgsql(_config.GetConnectionString("Postgres"))
                .EnableServiceProviderCaching(false)
                .EnableDetailedErrors(true);
            });

            services.AddLogging();
            services.AddScoped<ISeed, FileCryptDbContext>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IDatabaseTransaction, DatabaseTransaction>();
            services.AddScoped<IRedisCache, RedisCache>();
            services.AddScoped<IRedisDbContext, RedisDbContext>();
        }
    }
}
