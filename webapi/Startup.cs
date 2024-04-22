﻿using webapi.Middlewares;
using application;
using services;
using data_access;
using webapi.Helpers.Abstractions;
using webapi.Helpers;

namespace webapi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();

            AppConfigurationCheck.ConfigurationCheck();

            services.AddScoped<IUserInfo, UserData>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDataInfrastructure(config);
            services.AddServicesInfrastructure(config);
            services.AddApplication(config);

            AppServices.Register(services);
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
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseCors("AllowSpecificOrigin");
            app.UseAuthHandler();
            app.UseAuthentication();
            app.UseUserSession();

            if (env.IsDevelopment())
                app.UseLog();

            app.UseAuthorization();
            app.UseXSRF();
            app.UseExceptionHandle();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapControllers();
            });
        }
    }
}
