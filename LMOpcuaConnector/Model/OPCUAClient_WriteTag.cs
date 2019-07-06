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
        public void WriteTag(Tag tag, object value)
        {
            //non è ancora stata effettuata la lettura della tag
            if (tag.Type == null)
            {
                return;
                throw new NotImplementedException();
            }
            try
            {
                WriteValue valueToWrite = new WriteValue();

                valueToWrite.NodeId = tag.NodeId;
                valueToWrite.AttributeId = 13;
                valueToWrite.Value.Value = ChangeType(tag, value);
                valueToWrite.Value.StatusCode = StatusCodes.Good;
                valueToWrite.Value.ServerTimestamp = DateTime.MinValue;
                valueToWrite.Value.SourceTimestamp = DateTime.MinValue;

                WriteValueCollection valuesToWrite = new WriteValueCollection();
                valuesToWrite.Add(valueToWrite);

                // write current value.
                StatusCodeCollection results = null;
                DiagnosticInfoCollection diagnosticInfos = null;

                session.Write(
                    null,
                    valuesToWrite,
                    out results,
                    out diagnosticInfos);

                ClientBase.ValidateResponse(results, valuesToWrite);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, valuesToWrite);

                if (StatusCode.IsBad(results[0]))
                {
                    throw new ServiceResultException(results[0]);
                }
            }
            catch (Exception exception)
            {
                throw new NotImplementedException();
            }

        }
        public void WriteTag(string tagName, object value)
        {
            try
            {
#warning implementare se tag non esiste cosa fare
                Tag tag = ListOfTags.Tags.First(t => t.Name == tagName);
                if (tag != null)
                    WriteTag(tag, value);

            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }

        }
        private object ChangeType(Tag tag, object value)
        {
            object v = (value != null) ? value : null;

            switch (tag.Type.BuiltInType)
            {
                case BuiltInType.Boolean:
                    {
                        value = Convert.ToBoolean(v);
                        break;
                    }

                case BuiltInType.SByte:
                    {
                        value = Convert.ToSByte(v);
                        break;
                    }

                case BuiltInType.Byte:
                    {
                        value = Convert.ToByte(v);
                        break;
                    }

                case BuiltInType.Int16:
                    {
                        value = Convert.ToInt16(v);
                        break;
                    }

                case BuiltInType.UInt16:
                    {
                        value = Convert.ToUInt16(v);
                        break;
                    }

                case BuiltInType.Int32:
                    {
                        value = Convert.ToInt32(v);
                        break;
                    }

                case BuiltInType.UInt32:
                    {
                        value = Convert.ToUInt32(v);
                        break;
                    }

                case BuiltInType.Int64:
                    {
                        value = Convert.ToInt64(v);
                        break;
                    }

                case BuiltInType.UInt64:
                    {
                        value = Convert.ToUInt64(v);
                        break;
                    }

                case BuiltInType.Float:
                    {
                        value = Convert.ToSingle(v, new CultureInfo("en-US"));
                        break;
                    }

                case BuiltInType.Double:
                    {
                        value = Convert.ToDouble(v, new CultureInfo("en-US"));
                        break;
                    }

                default:
                    {
                        value = v;
                        break;
                    }
            }

            return value;
        }

    }
}
