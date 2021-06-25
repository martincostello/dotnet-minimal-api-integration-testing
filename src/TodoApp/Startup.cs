// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TodoApp.Data;
using TodoApp.Services;

namespace TodoApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IClock>((_) => NodaTime.SystemClock.Instance);
            services.AddScoped<ITodoRepository, TodoRepository>();
            services.AddScoped<ITodoService, TodoService>();

            services.AddRouting();

            services.AddGitHubAuthentication();

            services.AddRazorPages();

            services.AddDbContext<TodoContext>((serviceProvider, builder) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var dataDirectory = configuration["DataDirectory"];

                if (string.IsNullOrEmpty(dataDirectory) || !Path.IsPathRooted(dataDirectory))
                {
                    dataDirectory = Path.Combine(Environment.ContentRootPath, "App_Data");
                }

                var databaseFile = Path.Combine(dataDirectory, "TodoApp.db");

                builder.UseSqlite("Data Source=" + databaseFile);
            });

            services.AddHttpClient();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePages();
            }

            app.UseHsts();
            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints((endpoints) =>
            {
                endpoints.MapApiRoutes();
                endpoints.MapAuthenticationRoutes();
                endpoints.MapRazorPages();
            });
        }
    }
}
