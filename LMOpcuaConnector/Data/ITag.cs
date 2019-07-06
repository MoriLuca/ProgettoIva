using System;

namespace LMOpcuaConnector.Data
{
    public interface ITag
    {
        Opc.Ua.NodeId NodeId { get; set; }
        String Name { get; set; }
        object Value { get; set; }
        Opc.Ua.TypeInfo Type { get; set; }
        bool FirstRead { get; set; }
    }

}
