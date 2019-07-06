using System;
using Newtonsoft.Json;

namespace json
{
    class Program
    {
        static void Main(string[] args)
        {
            LMOpcuaConnector.Data.OPCUAInitializer init = new LMOpcuaConnector.Data.OPCUAInitializer();
            Console.WriteLine(JsonConvert.SerializeObject(init));
        }
    }
}
