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
        private void OnNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            foreach (var value in item.DequeueValues())
            {
                Tag tag = new Tag()
                {
                    NodeId = item.ResolvedNodeId,
                    Name = item.DisplayName,
                    Value = value.Value,
                    LastSync = value.SourceTimestamp,
                    StatusCode = value.StatusCode,
                    Type = value.WrappedValue.TypeInfo
                };

                if (ListOfTags.Tags.Exists(t => t.Name == tag.Name))
                {
                    int index = ListOfTags.Tags.FindIndex(t => t.Name == tag.Name);
                    ListOfTags.Tags[index] = tag;

                }
                else
                {
                    tag.FirstRead = true;
                    ListOfTags.Tags.Add(tag);
                }

                OnTagChange?.Invoke(this, tag);
            }


        }
    }
}
