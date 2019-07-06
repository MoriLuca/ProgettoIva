using LMOpcuaConnector.Data;

namespace LMOpcuaConnector.Data
{
    public class OPCUAInitializer
    {
        public string EndpointURL { get; set; }
        public bool AutoAccept { get; set; }
        public int StopTimeout { get; set; }
        public TagConfigurator TagConfigurator { get; set; }
        public ServerExportMethod ServerExportMethod { get; set; }
        public int PublishingInterval { get; set; } = 1000;
        public string RootTagsFolder { get; set; } = null;
    }
}
