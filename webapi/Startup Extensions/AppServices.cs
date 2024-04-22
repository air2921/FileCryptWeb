using application.Helpers;
using data_access.Ef;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace webapi
{
    public static class AppServices
    {
        public static void Register(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(typeof(Startup));
            services.AddControllers();
            services.AddLogging();
            services.AddHttpClient();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "webapi", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

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
                jwt.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = auth =>
                    {
                        auth.Response.StatusCode = 401;
                        auth.Response.ContentType = "application/json";
                        if (auth.Request.Cookies.ContainsKey(ImmutableData.REFRESH_COOKIE_KEY))
                            auth.Response.Headers.Append("X-AUTH-REQUIRED", true.ToString());

                        return auth.Response.WriteAsJsonAsync(new { message = "Invalid auth token" });
                    }
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
