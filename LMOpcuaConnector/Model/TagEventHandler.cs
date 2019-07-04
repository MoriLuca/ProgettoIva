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
        public EventHandler OnFineLotto;
        public EventHandler OnPezzoConcluso;

        public void TagUpdate(object sender, Tag tag)
        {
            switch (tag.Name)
            {
                case "FineLotto":
                    if ((bool)tag.Value == true) OnFineLotto?.Invoke(this, EventArgs.Empty);
                    break;
                case "Contapezzi":
                    if ((bool)tag.Value == true) OnPezzoConcluso?.Invoke(this, EventArgs.Empty);
                    break;
                default:
                    break;
            }
            //esemio, fire event per fine lotto
            if (tag.Name == "FineLotto" && (bool)tag.Value == true)
            {
                OnFineLotto?.Invoke(this, EventArgs.Empty);
            }


        }

    }
}
