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
            services.AddSingleton<LMOpcuaConnector.Model.OPCUAClient>(
                //s => new LMOpcuaConnector.OPCUAClient("opc.tcp://localhost:62541/discovery", true, Timeout.Infinite, LMOpcuaConnector.Data.TagConfigurator.FromTagClassList)
                s => new LMOpcuaConnector.Model.OPCUAClient("opc.tcp://192.168.250.10:48010/", true, Timeout.Infinite, LMOpcuaConnector.Data.TagConfigurator.BrowseTheServer
                , LMOpcuaConnector.Data.ServerExportMethod.Name, 1000, "Tags")
            );
            services.AddSingleton<LMOpcuaConnector.Model.OPCUATagEventHandler>();
            services.AddSingleton<ProjectWorker>();
            services.AddSingleton<LMLogger.Model.Logger>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //logger
            app.ApplicationServices.GetService<LMLogger.Model.Logger>()
                .UseConsole()
                .SetPath(@"C:\ProgramData\exware")
                .UseTxtFile();

            //Attiva il client OPCUA
            app.ApplicationServices.GetService<LMOpcuaConnector.Model.OPCUAClient>().Run();

            //Commentare queste opertazioni
            app.ApplicationServices.GetService<LMOpcuaConnector.Model.OPCUAClient>().OnTagChange +=
                 app.ApplicationServices.GetService<LMOpcuaConnector.Model.OPCUATagEventHandler>().TagUpdate;
            //evento gestione contapezzi
            app.ApplicationServices.GetService<LMOpcuaConnector.Model.OPCUATagEventHandler>().OnPezzoConcluso +=
                app.ApplicationServices.GetService<ProjectWorker>().ContaPezziHandler;


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
