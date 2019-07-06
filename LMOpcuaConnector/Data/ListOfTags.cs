using System.Collections.Generic;
using System.Linq;

namespace LMOpcuaConnector.Data
{
    public class ListOfTags : IListOfTags
    {
        //public static readonly string Stringa = "Stringa";
        //public static readonly string Numeraccio = "Numeraccio";
        //public static readonly string Intero = "Intero";

        public List<Tag> Tags { get; set; }

        public ListOfTags()
        {
            Tags = new List<Tag>
            {
                //new Tag(){ NodeId = "ns=4;i=2258", Name = "ServerCurrentTime" },
                //new Tag(){ NodeId = "ns=4;i=2256", Name = "ServerStatus" },
                //new Tag(){ NodeId = "ns=4;i=1279", Name = "FCSetopint" },


                //new Tag(){ NodeId = "ns=2;s=[default]/Tags/Stringa", Name = "Stringa" },
                //new Tag(){ NodeId = "ns=2;s=[default]Tags/Numeraccio", Name = "Numeraccio" },
                //new Tag(){ NodeId = "ns=2;s=[default]Tags/Intero", Name = "Intero" },
                //new Tag(){ NodeId = "ns=2;s=[default]Tags/RiepilogoAllarme", Name = "RiepilogoAllarme" },
                //new Tag(){ NodeId = "ns=2;s=[default]Tags/Allarme1", Name = "Allarme1" },
                //new Tag(){ NodeId = "ns=2;s=[default]Tags/Allarme2", Name = "Allarme2" },
                //new Tag(){ NodeId = "ns=2;s=[default]Tags/Allarme3", Name = "Allarme3" },
                //new Tag(){ NodeId = "ns=2;s=[default]Tags/Allarme4", Name = "Allarme4" },
                //new Tag(){ NodeId = "ns=2;s=[default]Tags/Allarme5", Name = "Allarme5" },
            };
        }

        public Tag GetTagByName(string Name)
        {
            try
            {
                return Tags.FirstOrDefault(t => t.Name == Name);
            }
            catch (System.Exception)
            {

#warning implemetnare
                return null;
            }
            
        }
    }

    public interface IListOfTags
    {
        List<Tag> Tags { get; set; }
        Tag GetTagByName(string Name);
    }

}
