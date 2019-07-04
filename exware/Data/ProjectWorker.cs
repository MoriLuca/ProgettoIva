using System;
using System.Threading.Tasks;

namespace exware.Data
{
    public class ProjectWorker
    {
        LMOpcuaConnector.Model.OPCUAClient opc;

        public ProjectWorker(LMOpcuaConnector.Model.OPCUAClient _opc)
        {
            opc = _opc;
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
                Task.Run(()=>
                {
                    //reset della tag contapezzi
                    opc.WriteTag("Contapezzi", false);
                    Console.WriteLine("reset tag contapezzi terminata");
                });
                Console.WriteLine("reset tag contapezzi lanciata");

            }
            
        }
    }
}
