using System;
using LMOpcuaConnector.Data;

namespace LMOpcuaConnector.Model
{
    /// <summary>
    ///Questa classe si proccupa di intercettare gli eventi riguardanti il cambio di stato delle tags, 
    ///ed eventualmente lanciare l'evento riguardante una singola tag
    /// </summary>
    public  class OPCUATagEventHandler
    {
        private readonly LMLogger.Model.Logger _logger;
        //eventi boolenani, non richiedono il passaggio di informazioni
        public EventHandler OnFineLotto;
        public EventHandler OnPezzoConcluso;
        public EventHandler<Tag> OnContapezziChange;

        public OPCUATagEventHandler(LMLogger.Model.Logger logger)
        {
            _logger = logger;
        }

        public void TagUpdate(object sender, Tag tag)
        {

            try
            {
                switch (tag.Name)
                {
                    case "FineLotto":
                        if ((bool)tag.Value == true) OnFineLotto?.Invoke(this, EventArgs.Empty);
                        break;
                    case "Contapezzi":
                        if ((bool)tag.Value == true) OnPezzoConcluso?.Invoke(this, EventArgs.Empty);
                        break;
                    case "NumeroPezziProdottiProgramma_1":
                        OnContapezziChange?.Invoke(this, tag);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInfo(this,$"Tag Update event for {tag.Name}, throw exception : {ex.Message}");
            }
            
        }

    }
}
