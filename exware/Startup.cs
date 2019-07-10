using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using exware.Data;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

namespace exware
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddDbContext<Db>(ServiceLifetime.Singleton);
            services.AddSingleton<LMOpcuaConnector.Model.OPCUAClient>();
            services.AddSingleton<LMOpcuaConnector.Model.OPCUATagEventHandler>();
            services.AddSingleton<LMLogger.Model.Logger>();
            services.AddSingleton<LMEmail.Model.EmailHandler>();
            services.AddSingleton<EventHandlerLinker>();
            services.AddHostedService<Services.Worker>();

            services
    .AddBlazorise(options =>
    {
        options.ChangeTextOnKeyPress = true; // optional
    })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
  .UseBootstrapProviders()
  .UseFontAwesomeIcons();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

        }
    }
}
