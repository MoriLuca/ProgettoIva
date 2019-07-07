using System;
using System.Threading.Tasks;
using LMLogger.Model;
using LMOpcuaConnector.Model;
using LMEmail.Model;

namespace exware.Data
{
    public class EventHandlerLinker
    {
        private readonly OPCUAClient opc;
        private readonly Logger logger;
        private readonly EmailHandler email;

        #region ctor
        public EventHandlerLinker(OPCUAClient _opc,Logger _logger, EmailHandler _email)
        {
            opc = _opc;
            logger = _logger;
            email = _email;
        }
        #endregion

        //Subscribe all'evento contapezzi aggiornato.
        //Il contapezzi viene settato a true dal PLC, deve essere resettato dal PC
        public void ContaPezziHandler(object sender, EventArgs e)
        {
            try
            {
                logger.LogInfo(this, "Contappezzi intercettato, tag resettata");
                //email.SendEmailWithDefaultSettings("Contapezzi", "<h2>Contappezzi incrementato.</h2>", true);
                opc.WriteTag("Contapezzi", false);
            }
            catch
            {
                //await email.SendEmailWithDefaultSettings("Contapezzi", "<h2>Contappezzi incrementato.</h2>", true);
            }

        }

        public void ContapezziHasChanged(object sender, LMOpcuaConnector.Data.Tag tag)
        {
            logger.LogInfo(this, $"Il valore del contapezzi per il programma 1 vale : {tag.Value}");
        }

    }
}
