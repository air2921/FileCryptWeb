using data.Abstractions;
using data.Ef;
using data.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using shared.Immutable;

namespace data
{
    public static class ServiceRegistration
    {
        public static void AddDataInfrastructure(this IServiceCollection services, IConfiguration _config)
        {
            services.Configure<RedisDbContext>(options =>
            {
                options.ConnectionString = _config.GetConnectionString(App.REDIS_DB)!;
            });

            services.AddDbContext<FileCryptDbContext>(options =>
            {
                options.UseNpgsql(_config.GetConnectionString(App.MAIN_DB))
                .EnableServiceProviderCaching(false)
                .EnableDetailedErrors(true);
            });

            using var serviceScope = services.BuildServiceProvider().CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetService<FileCryptDbContext>();
            dbContext.Initial();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IDatabaseTransaction, DatabaseTransaction>();
            services.AddScoped<IRedisDbContext, RedisDbContext>();
            services.AddScoped<IRedisCache, RedisCache>();
        }
    }
}
