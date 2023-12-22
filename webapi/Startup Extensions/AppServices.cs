using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using webapi.DB;
using webapi.Services;

namespace webapi
{
    public class AppServices
    {
        public static void Register(IServiceCollection services)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddUserSecrets<AppServices>()
                .Build();

            services.AddDbContext<FileCryptDbContext>(options => options.UseNpgsql(configuration.GetConnectionString(App.MainDb)));

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

                options.AddPolicy("AllowOriginAPI", builder =>
                {
                    builder.AllowAnyOrigin().WithMethods("POST").WithHeaders("x-Encryption_Key", "x-API_Key")
                    .SetIsOriginAllowed(origin =>
                    origin.EndsWith("api/public/cryptography/private/decryption") ||
                    origin.EndsWith("api/public/cryptography/internal/decryption") ||
                    origin.EndsWith("api/public/cryptography/received/decryption")
                    );
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration[App.appSecretKey]!)),
                    ValidIssuer = "FileCrypt",
                    ValidAudience = "User",
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAntiforgery(options => { options.HeaderName = "X-XSRF-TOKEN"; });
            services.AddMvc();

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 75 * 1024 * 1024;
            });
        }
    }
}
