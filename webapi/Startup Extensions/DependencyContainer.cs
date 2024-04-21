using webapi.Helpers;
using webapi.Helpers.Abstractions;

namespace webapi
{
    public static class DependencyContainer
    {
        public static void Singleton(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static void Scoped(this IServiceCollection services)
        {
            services.AddScoped<IUserInfo, UserData>();
        }
    }
}
