using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LMOpcuaConnector.Data;
using System.Linq;
using System.IO;
using System.Globalization;

namespace LMOpcuaConnector.Model
{
    public partial class OPCUAClient
    {
        public object ReadTag(Tag tag)
        {
            return session.ReadValue(tag.NodeId).WrappedValue.Value;
        }
        public object ReadTag(string tagName)
        {
            return ReadTag(ListOfTags.Tags.FirstOrDefault(n=>n.Name == tagName));
        }

    }
}
