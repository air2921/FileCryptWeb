using webapi.Middlewares;

namespace webapi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            AppConfigurationCheck.ConfigurationCheck();

            DependencyContainer.Singleton(services);
            DependencyContainer.Scoped(services);
            DependencyContainer.Transient(services);
            DependencyContainer.OtherServices(services);

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
            app.UseFreeze();
            app.UseBearer();
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
