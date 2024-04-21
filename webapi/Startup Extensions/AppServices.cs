using application.Helpers;
using data_access.Ef;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace webapi
{
    public class AppServices
    {
        public static void Register(IServiceCollection services)
        {
            using var serviceScope = services.BuildServiceProvider().CreateScope();
            var dbSeed = serviceScope.ServiceProvider.GetRequiredService<ISeed>();
            dbSeed.AdminSeed();

            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();

            services.AddAutoMapper(typeof(Startup));
            services.AddControllers();
            services.AddLogging();
            services.AddHttpClient();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder =>
                {
                    builder.WithOrigins("https://localhost:5173")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                });
            });

            services.AddDistributedMemoryCache();

            services.AddSession(session =>
            {
                session.IdleTimeout = TimeSpan.FromMinutes(15);
                session.Cookie.HttpOnly = true;
                session.Cookie.SameSite = SameSiteMode.None;
                session.Cookie.IsEssential = true;
                session.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminPolicy", policy =>
                {
                    policy.RequireRole("HighestAdmin", "Admin");
                });
            });

            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwt =>
            {
                jwt.RequireHttpsMetadata = true;
                jwt.SaveToken = true;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration[App.SECRET_KEY]!)),
                    ValidIssuer = configuration[App.ISSUER],
                    ValidAudience = configuration[App.AUDIENCE],
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAntiforgery(options => { options.HeaderName = ImmutableData.XSRF_HEADER_NAME; });
            services.AddMvc();

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 75 * 1024 * 1024;
            });
        }
    }
}
