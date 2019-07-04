namespace LMOpcuaConnector.Data
{
    /// <summary>
    /// Elenco di modi disponibili per la laettura e il subscribe delle tags
    /// </summary>
    public enum TagConfigurator : int
    {
        BrowseTheServer,
        ReadJSON,
        FromTagClassList
    };
}
