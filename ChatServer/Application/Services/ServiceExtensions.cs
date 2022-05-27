using ChatServer.Domain.Entities;
using ChatServer.Infrstructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Application.Services
{
    public static class ServiceExtensions
    {
        public static void ConfigureIdentity(this IServiceCollection services)
        {
            var builder = services.AddIdentityCore<ChatUser>();

            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), services);
            builder.AddEntityFrameworkStores<ChatDataContext>().AddDefaultTokenProviders();
        }

        public static void ConfigureJWT2(this IServiceCollection services, IConfiguration Configuration)
        {
            var settings = Configuration.GetSection("jwtSettings");
            var key = settings.GetSection("KEY").Value; // better set env variable and use Enviroment.GetEnviromentVariable("variableName")
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                  .AddJwtBearer(options =>
                  {
                      options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            ValidateIssuer = false,
                            ValidateActor = false,
                            ValidateLifetime = false,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? "2ec6f741-4dd9-4858-a0d7-b041afa994f0"))
                        };
                  });

        }
        public static void ConfigureJWT(this IServiceCollection services, IConfiguration Configuration)
        {
            var settings = Configuration.GetSection("jwtSettings");
            var key = settings.GetSection("KEY").Value; // better set env variable and use Enviroment.GetEnviromentVariable("variableName")
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateActor = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? "2ec6f741-4dd9-4858-a0d7-b041afa994f0"))
                };


                 options.RequireHttpsMetadata = false;

                  options.Events = new JwtBearerEvents
                  {
                      OnMessageReceived = context =>
                     {
                         var htoken = context.Request.Headers["Authorization"];
                         var accessToken = context.Request.Query["access_token"];
                         // If the request is for our hub...
                         var path = context.HttpContext.Request.Path;
                         context.Token = accessToken;
                         Console.WriteLine($"Token {accessToken} - {htoken}");
                         if (!string.IsNullOrEmpty(accessToken) &&
                             (path.StartsWithSegments("/chathub")))
                         {
                             // Read the token out of the query string
                             context.Token = accessToken;
                         }
                         return Task.CompletedTask;
                     }
                  };
            });

        }
    }
}

