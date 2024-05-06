using webapi.Middlewares;
using application;
using services;
using data_access;
using webapi.Helpers.Abstractions;
using webapi.Helpers;
using Serilog;
using application.Helpers;
using Serilog.Sinks.Elasticsearch;
using additional;

namespace webapi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Startup>()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Environment", env)
                .ReadFrom.Configuration(config)
                .WriteTo.Console()
                .WriteTo.Elasticsearch(ConfigurationElasticSink(config))
                .CreateLogger();

            config.Check();
            services.AddScoped<IUserInfo, UserData>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddLogging(log =>
            {
                log.ClearProviders();
                log.AddSerilog(Log.Logger);
            });

            services.AddProviderInfrastructure(Log.Logger);
            services.AddDataInfrastructure(config, Log.Logger);
            services.AddServiceInfrastructure(config, Log.Logger);
            services.AddApplication(config);
            services.AddServices(config);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if(env.IsProduction())
                app.UseHsts();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseSession();
            app.UseCors("AllowSpecificOrigin");
            app.UseBearer();
            app.UseAuthentication();
            app.UseUserSession();
            app.UseLog();
            app.UseAuthorization();
            app.UseXSRF();
            app.UseExceptionHandle();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapControllers();
            });
        }

        private static ElasticsearchSinkOptions ConfigurationElasticSink(IConfigurationRoot configuration)
        {
            return new ElasticsearchSinkOptions(new Uri(configuration.GetConnectionString(App.ELASTIC_SEARCH)!))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"logs-{DateTime.UtcNow:yyyy}"
            };
        }
    }
}
