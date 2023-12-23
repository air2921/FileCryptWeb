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

            // If you need to receive logs from authorized requests, and receive information about who the request came from,
            // you need to move this middleware to after the UseAuthorization middleware

            app.UseFreeze();
            app.UseAPI();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseCors("AllowSpecificOrigin");
            app.UseBearer();
            app.UseAuthentication();
            app.UseLog();
            app.UseAuthorization();
            app.UseXSRF();
            // u can call 'UseLog' here

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapControllers();
            });
        }
    }
}
