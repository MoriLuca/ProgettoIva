using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpcuaAsService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {

                services.AddSingleton<LMOpcuaConnector.Model.OPCUAClient>(
                    s => new LMOpcuaConnector.Model.OPCUAClient("opc.tcp://192.168.250.10:48010/", true, Timeout.Infinite, LMOpcuaConnector.Data.TagConfigurator.BrowseTheServer
                    , LMOpcuaConnector.Data.ServerExportMethod.Name, 1000, "Tags"));

                    services.AddHostedService<Worker>();
    });
    }
}
