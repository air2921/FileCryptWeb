using Serilog;

namespace webapi
{
    public class Program
    {
        static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging(a => { a.ClearProviders(); })
            .UseSerilog()
            .ConfigureWebHostDefaults(builder =>
            {
                builder.UseStartup<Startup>();
            });
    }
}
