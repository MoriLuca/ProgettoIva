using System;
using System.Threading.Tasks;
using LMLogger.Model;
using LMOpcuaConnector.Model;

namespace exware.Data
{
    public class EventHandlerLinker
    {
        private readonly OPCUAClient opc;
        private readonly Logger logger;

        #region ctor
        public EventHandlerLinker(OPCUAClient _opc,Logger _logger)
        {
            opc = _opc;
            logger = _logger;
        }
        #endregion

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
                });
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
                });
                logger.LogInfo(this, "Contappezzi intercettato, tag resettata");

            }

        }

        public void ContapezziHasChanged(object sender, LMOpcuaConnector.Data.Tag tag)
        {
            logger.LogInfo(this, $"Il valore del contapezzi per il programma 1 vale : {tag.Value}");
        }

    }
}
