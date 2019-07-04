using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace test
{
    class Program
    {
        public static ServiceProvider services;
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            //timeout -1 == infinite
            serviceCollection.AddScoped<LMOpcuaConnector.Model.OPCUAClient>(
                s => new LMOpcuaConnector.Model.OPCUAClient("opc.tcp://192.168.250.10:48010/", true, -1, LMOpcuaConnector.Data.TagConfigurator.BrowseTheServer)
                );
            serviceCollection.AddScoped<LMOpcuaConnector.Model.OPCUATagEventHandler>();
            serviceCollection.AddScoped<ProjectWorker>();
            services = serviceCollection.BuildServiceProvider();

            //in questo modo la getsione dei cambi di stato viene delegata alla classe apposita
            services.GetService<LMOpcuaConnector.Model.OPCUAClient>().OnTagChange +=
                services.GetService<LMOpcuaConnector.Model.OPCUATagEventHandler>().TagUpdate;


            services.GetService<LMOpcuaConnector.Model.OPCUAClient>().OnTagChange += TagChanged;

            //Subscribe di eventi specifici
            services.GetService<LMOpcuaConnector.Model.OPCUATagEventHandler>().OnFineLotto += FineLotto;
            services.GetService<LMOpcuaConnector.Model.OPCUATagEventHandler>().OnPezzoConcluso += services.GetService<ProjectWorker>().ContaPezziHandler;
            // fine subscribe


            services.GetService<LMOpcuaConnector.Model.OPCUAClient>().Run();


            //services.Dispose();
            Console.ReadLine();
        }

        public static void TagChanged(object sender, LMOpcuaConnector.Data.Tag tag)
        {
            //Console.WriteLine($"{tag.Name} : {tag.Value}");
        }

        public static void FineLotto(object sender, EventArgs e)
        {
            //Console.WriteLine("la fine del lotto è arrivata.");
            //Console.WriteLine(services.GetService<LMOpcuaConnector.Model.OPCUAClient>().ReadTag("FineLotto"));
        }
    }
}
