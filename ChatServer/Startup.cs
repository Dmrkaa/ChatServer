using System;
using System.IO;
using System.Net;
using System.Linq;
using ChatServer.Hubs;
using System.Net.Sockets;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using ChatServer.Infrstructure.Data;
using Microsoft.EntityFrameworkCore;
using ChatServer.Application.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Extensions;
using ChatServer.Infrstructure.Repositories;
using ChatServer.Domain.Repository_Interfaces;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer
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
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            services.AddAuthentication();
            services.AddSignalR();
            services.AddAutoMapper(typeof(UserMapper));
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IAuthManager, AuthManager>();
            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
            });
            services.AddDbContext<ChatDataContext>(options =>
                                 options.UseSqlServer(
                                     Configuration.GetConnectionString("DefaultConnection"),
                                     b => b.MigrationsAssembly(typeof(ChatDataContext).Assembly.FullName)));
            services.ConfigureIdentity();
            services.ConfigureJWT(Configuration);

            services.AddMemoryCache();

            services.AddControllers().AddNewtonsoftJson(
                    js => js.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            AddSwaggerDoc(services);
        }

        private void AddSwaggerDoc(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("BearerJwt", new OpenApiSecurityScheme
                {
                    Description = @"JWT Auth header using bearer.
                    Enter 'Bearer [space] and then token in the text input below'.
                    Example: 'Bearer your12321token'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "Oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                    }
                });

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ChatServer",
                    Version = "v1",
                    Description = "Test task"
                });
                /*  var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                  c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));*/
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseForwardedHeaders();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChatServer v1"));
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chathub");

                endpoints.MapGet("/", async context =>
                {
                    context.Response.ContentType = "text/plain";

                    // Host info
                    var name = Dns.GetHostName(); // get container id
                    var ip = Dns.GetHostEntry(name).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    Console.WriteLine($"Host Name: { Environment.MachineName} \t {name}\t {ip}");
                    await context.Response.WriteAsync($"Host Name: {Environment.MachineName}{Environment.NewLine}");
                    await context.Response.WriteAsync(Environment.NewLine);

                    // Request method, scheme, and path
                    await context.Response.WriteAsync($"Request Method: {context.Request.Method}{Environment.NewLine}");
                    await context.Response.WriteAsync($"Request Scheme: {context.Request.Scheme}{Environment.NewLine}");
                    await context.Response.WriteAsync($"Request URL: {context.Request.GetDisplayUrl()}{Environment.NewLine}");
                    await context.Response.WriteAsync($"Request Path: {context.Request.Path}{Environment.NewLine}");

                    // Headers
                    await context.Response.WriteAsync($"Request Headers:{Environment.NewLine}");
                    foreach (var (key, value) in context.Request.Headers)
                    {
                        await context.Response.WriteAsync($"\t {key}: {value}{Environment.NewLine}");
                    }
                    await context.Response.WriteAsync(Environment.NewLine);

                    // Connection: RemoteIp
                    await context.Response.WriteAsync($"Request Remote IP: {context.Connection.RemoteIpAddress}");
                });

            });
        }
    }
}
