using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebInterface.Docker;

namespace WebInterface
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, DockerService dockerService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseSignalR();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            dockerService.MonitorEvents();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddSignalR(options =>
            {
                options.Hubs.EnableDetailedErrors = true;
            });

            var dockerAddress = Environment.GetEnvironmentVariable("DOCKER_REMOTE_API");

            if (string.IsNullOrEmpty(dockerAddress))
            {
                //services.Configure<DockerHost>(Configuration.GetSection("DockerHostTest"));
                throw new Exception("DOCKER_REMOTE_API environment variable not found");
            }
            else
            {
                services.Configure<DockerHost>(dockerHost =>
                {
                    dockerHost.Uri = dockerAddress;
                });
            }

            services.AddSingleton<DockerService>();
        }
    }
}
