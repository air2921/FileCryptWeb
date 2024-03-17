using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using webapi.DB;
using webapi.Helpers;

namespace webapi
{
    public class AppServices
    {
        public static void Register(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();

            services.AddDbContext<FileCryptDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString(App.MAIN_DB))
                .EnableServiceProviderCaching(false)
                .EnableDetailedErrors(true);
            });

            using var serviceScope = services.BuildServiceProvider().CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetService<FileCryptDbContext>();
            dbContext.Initial();

            services.AddAutoMapper(typeof(Startup));
            services.AddControllers();
            services.AddLogging();
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

                options.AddPolicy("AllowAnyOrigin", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });

                options.AddPolicy("AllowOriginAPI", builder =>
                {
                    builder.AllowAnyOrigin()
                    .WithMethods("POST")
                    .WithHeaders(ImmutableData.ENCRYPTION_KEY_HEADER_NAME, ImmutableData.API_HEADER_NAME)
                    .SetIsOriginAllowed(origin =>
                    origin.EndsWith("api/public/cryptography/private/decrypt") ||
                    origin.EndsWith("api/public/cryptography/internal/decrypt") ||
                    origin.EndsWith("api/public/cryptography/received/decrypt") ||
                    origin.EndsWith("api/public/cryptography/private/encrypt") ||
                    origin.EndsWith("api/public/cryptography/internal/encrypt") ||
                    origin.EndsWith("api/public/cryptography/received/encrypt"));
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

            services.AddAuthorization();

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
                    ValidIssuer = "FileCrypt",
                    ValidAudience = "User",
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
