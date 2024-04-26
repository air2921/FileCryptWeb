using application.Helpers;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace webapi
{
    public class Program
    {
        static void Main(string[] args)
        {
            //var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            //var config = new ConfigurationBuilder()
            //    .AddUserSecrets<Program>()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json", false, true)
            //    .AddEnvironmentVariables()
            //    .Build();

            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Information()
            //    .Enrich.FromLogContext()
            //    .Enrich.WithProperty("Environment", env)
            //    .ReadFrom.Configuration(config)
            //    .WriteTo.Console()
            //    .WriteTo.Elasticsearch(ConfigurationElasticSink(config, env!))
            //    .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        //private static ElasticsearchSinkOptions ConfigurationElasticSink(IConfigurationRoot configuration, string env)
        //{
        //    return new ElasticsearchSinkOptions(new Uri(configuration.GetConnectionString(App.ELASTIC_SEARCH)!))
        //    {
        //        AutoRegisterTemplate = true,
        //        IndexFormat = $"logs-{DateTime.UtcNow:yyyy}"
        //    };
        //}

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
