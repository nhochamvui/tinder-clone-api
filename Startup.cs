using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using System.Threading.Tasks;
using TinderClone.Hubs;
using TinderClone.Infrastructure;
using TinderClone.Models;
using TinderClone.Services;
using TinderClone.Singleton;

namespace TinderClone
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            Console.WriteLine("--> Running on: " + env.ToString());

            if (env.Equals("Development"))
            {
                services.AddDbContext<TinderContext>(opt => opt.UseNpgsql(Configuration.GetConnectionString("TinderContext")));
            }
            else
            {
                string connectionUrl = Environment.GetEnvironmentVariable("DATABASE_URL").Replace("postgres://", string.Empty);
                Console.WriteLine("************Start**************");
                Console.WriteLine("DATABASE_URL: " + connectionUrl);
                Console.WriteLine("************End**************");

                var pgUserPass = connectionUrl.Split("@")[0];
                var pgHostPortDb = connectionUrl.Split("@")[1];

                var pgHostPort = pgHostPortDb.Split("/")[0];
                var pgHost = pgHostPort.Split(":")[0];
                var pgPort = pgHostPort.Split(":")[1];
                var pgDb = pgHostPortDb.Split("/")[1];

                var pgUser = pgUserPass.Split(":")[0];
                var pgPass = pgUserPass.Split(":")[1];
                var connectionString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Require;Trust Server Certificate=true";
                //var connectionString = $"Host={host};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";

                // add DB context
                services.AddDbContext<TinderContext>(opt => opt.UseNpgsql(connectionString));
            }

            services.AddControllers();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/chat")))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSignalR();

            services.AddSingleton<IUserIdProvider, UserIDProvider>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IFacebookService, FacebookService>();
            services.AddTransient<ILocationService, LocationService>();
            services.AddTransient<IUsersRepository, UsersRepository>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    var origins = Configuration["CorsOrigins"].ToString()
                    .Split(";", StringSplitOptions.TrimEntries);
                    builder.WithOrigins(origins)
                    .AllowCredentials()
                    .AllowAnyHeader().AllowAnyMethod()
                    .WithExposedHeaders("location");
                });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TinderClone", Version = "v1" });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TinderContext tinderContext)
        {
            Console.WriteLine("--> Connection String: " + tinderContext.Database.GetConnectionString());
            Console.WriteLine("--> Is database can connect: " + tinderContext.Database.CanConnect());
            try
            {
                Console.WriteLine("--> Running migration");
                tinderContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("************Start Trace**************");
                Console.WriteLine("Exception during migrate database: " + ex.Message);
                Console.WriteLine("************End Trace**************");
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<Chat>("/chat");
            });
        }
    }
}
