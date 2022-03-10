using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TinderClone.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TinderClone.Hubs;
using Microsoft.AspNetCore.SignalR;
using TinderClone.Singleton;
using System.IdentityModel.Tokens.Jwt;
using TinderClone.Services;

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
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")??"Development";
            Console.WriteLine("-->Running on: " + env.ToString());

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
                            var tokenHandler = new JwtSecurityTokenHandler();
                            var userRaw = tokenHandler.ReadJwtToken(accessToken);
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

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                    .WithOrigins(new string[] {
                        "https://tinderclonev2.netlify.app/",
                        "https://tinderclonev2.netlify.app",
                        "http://192.168.1.8:8080", 
                        "https://192.168.1.8:8080", "https://localhost:8080", 
                        "http://localhost:8080" })
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
            Console.WriteLine("--> Running migration");
            Console.WriteLine("--> Connection String: " + tinderContext.Database.GetConnectionString());
            Console.WriteLine("--> Is database can connect: " + tinderContext.Database.CanConnect());
            try
            {
                tinderContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("************Start**************");
                Console.WriteLine("Exception during migrate database: " + ex.Message);
                Console.WriteLine("************End**************");
            }
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TinderClone v1"));
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            //app.UseCors(options => options.WithOrigins("http://localhost:8080").AllowAnyHeader().AllowAnyMethod());
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
