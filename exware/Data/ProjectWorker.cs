using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace exware.Data
{
    public class ProjectWorker
    {
        LMOpcuaConnector.Model.OPCUAClient opc;
        private readonly LMLogger.Model.Logger logger;

        public ProjectWorker(LMOpcuaConnector.Model.OPCUAClient _opc,
                             LMLogger.Model.Logger _logger)
        {
            opc = _opc;
            logger = _logger;
        }


        //Subscribe all'evento contapezzi aggiornato.
        //Il contapezzi viene settato a true dal PLC, deve essere resettato dal PC
        public void ContaPezziHandler(object sender, EventArgs e)
        {
            try
            {
                //lettura parametri da OPC
                //...
                int pezzi = 70;
                int temip = 42;
                //scrittura dei parametri su database
                //..
                Task.Run(() =>
                {
                    //reset della tag contapezzi
                    Task.Delay(5000).Wait();
                    Console.WriteLine("scrittura db terminata");
                });
                Console.WriteLine("scrittura db lanciata");
            }
            catch (Exception)
            {
                throw new NotImplementedException();
            }
            finally
            {
                Task.Run(() =>
                {
                    //reset della tag contapezzi
                    opc.WriteTag("Contapezzi", false);
                    Console.WriteLine("reset tag contapezzi terminata");
                });
                logger.LogInfo(this, "Contappezzi intercettato, tag resettata");

            }

        }

    }
}
