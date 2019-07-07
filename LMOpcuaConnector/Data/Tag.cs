using System;

namespace LMOpcuaConnector.Data
{
    public class Tag : ITag
    {
        public Opc.Ua.NodeId NodeId { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
        public Opc.Ua.TypeInfo Type { get; set; }
        public DateTime LastSync { get; set; }
        public Opc.Ua.StatusCode StatusCode { get; set; }
        public bool FirstRead { get; set; }

    }

}
