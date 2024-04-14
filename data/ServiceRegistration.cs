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

            using var serviceScope = services.BuildServiceProvider().CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetService<FileCryptDbContext>();
            dbContext.Initial();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IDatabaseTransaction, DatabaseTransaction>();
            services.AddScoped<IRedisCache, RedisCache>();
            services.AddScoped<IRedisDbContext, RedisDbContext>();
        }
    }
}
