using webapi.Middlewares;

namespace webapi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            AppConfigurationCheck.ConfigurationCheck();

            DiContainer.Singleton(services);
            DiContainer.Scoped(services);
            DiContainer.Transient(services);

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

            app.UseFreeze();
            app.UseAPI();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseCors("AllowSpecificOrigin");
            app.UseBearer();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseLog();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapControllers();
            });
        }
    }
}
