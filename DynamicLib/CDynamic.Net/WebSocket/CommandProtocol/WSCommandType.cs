using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute("WSCommand", Namespace = "", IsNullable = false)]
    public partial class WSCommandType : WSCommandTypeBase
    {

    //    private string requestIDField;

        private WSCommandTypeParameter[] parametersField;

        /// <remarks/>
        //public string RequestID
        //{
        //    get
        //    {
        //        return this.requestIDField;
        //    }
        //    set
        //    {
        //        this.requestIDField = value;
        //    }
        //}


        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Parameter", IsNullable = false)]
        public WSCommandTypeParameter[] Parameters
        {
            get
            {
                return this.parametersField;
            }
            set
            {
                this.parametersField = value;
            }
        }


        public override string ParserID
        {
            get { return "XML"; }
        }

        public override MessageContentType TransferEncoder
        {
            get { return MessageContentType.Text; }
        }

        public override bool CanRead(MessageReceivedEventArgs args, out WSCommandTypeBase command)
        {
            command = null;
            if (args.ContentType == TransferEncoder)
            {
                if (!String.IsNullOrEmpty(args.Content) && args.Content.StartsWith("{XML}"))
                {
                    command = Dynamic.Core.Runtime.SerializationUtility.ToObject<WSCommandType>(args.Content.Substring(5));
                    return true;
                }
            }

            return false;
        }

        public override void SetReplyCommand(MessageReceivedEventArgs args, WSCommandTypeBase command)
        {
            args.Content = "{XML}" + Dynamic.Core.Runtime.SerializationUtility.ToXmlString(command);
        }

        public override WSCommandTypeBase Create()
        {
            return new WSCommandType();
        }

        public override void CreateParameters(int count)
        {
            Parameters = new WSCommandTypeParameter[count];
        }

        public override void AddParameter(string name, int index)
        {
            Parameters[index] = new WSCommandTypeParameter() { Name = name, Value = "" };
        }

        public override int ParameterCount
        {
            get
            {
                return (Parameters == null ? 0 : Parameters.Length);
            }
        }

        public override bool HasParameter(string name)
        {
            if (Parameters == null)
                return false;

            return Parameters.Any(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public override bool IsPairCommand(WSCommandTypeBase command)
        {
            if (command is WSCommandType)
            {
                return (command as WSCommandType).RequestID == this.RequestID;
            }

            return false;
        }

        public override void SetCommandParameter(string name, string value)
        {
            WSCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                throw new NotSupportedException();
            }

            par.Value = value;
        }

        public override void SetCommandParameter(string name, byte[] value)
        {
            WSCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                throw new NotSupportedException();
            }

            par.Value = Convert.ToBase64String(value);

        }

        public override void SetCommandParameter(string name, int value)
        {
            WSCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                throw new NotSupportedException();
            }

            par.Value = value.ToString();

        }

        public override void SetCommandParameter(string name, long value)
        {
            WSCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                throw new NotSupportedException();
            }

            par.Value = value.ToString();
        }

        public override void SetCommandParameter(string name, decimal value)
        {
            WSCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                throw new NotSupportedException();
            }

            par.Value = value.ToString();
        }

        public override void SetCommandParameter(string name, bool value)
        {
            WSCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                throw new NotSupportedException();
            }

            par.Value = value.ToString();
        }


        public override void SetCommandParameter(string name, object value)
        {
            string xml = Dynamic.Core.Runtime.SerializationUtility.ToXmlString(value);
            SetCommandParameter(name, xml);
        }




        public override string GetCommandParameterStringValue(string name)
        {
            WSCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                return "";
            }

            return par.Value;
        }

        public override byte[] GetCommandParameterBinaryValue(string name)
        {
            string strValue = GetCommandParameterStringValue(name);
            if (strValue == null)
                return null;

            return Convert.FromBase64String(strValue);
        }



        public override int GetCommandParameterIntValue(string name)
        {
            string strValue = GetCommandParameterStringValue(name);
            if (strValue == null)
                return 0;

            return Convert.ToInt32(strValue);
        }

        public override long GetCommandParameterLongValue(string name)
        {
            string strValue = GetCommandParameterStringValue(name);
            if (strValue == null)
                return 0;

            return Convert.ToInt64(strValue);

        }

        public override decimal GetCommandParameterDecimalValue(string name)
        {
            string strValue = GetCommandParameterStringValue(name);
            if (strValue == null)
                return 0;

            return Convert.ToDecimal(strValue);

        }

        public override Boolean GetCommandParameterBooleanValue(string name)
        {
            string strValue = GetCommandParameterStringValue(name);
            if (strValue == null)
                return false;

            return Convert.ToBoolean(strValue);

        }

        public override T GetCommandParameterObjectValue<T>(string name)
        {
            string xml = GetCommandParameterStringValue(name);
            if (String.IsNullOrEmpty(xml))
            {
                return default(T);
            }

            return Dynamic.Core.Runtime.SerializationUtility.ToObject<T>(xml);
        }


        public override byte[] ToBinary(WSCommandTypeBase command)
        {
            return Encoding.UTF8.GetBytes("{XML}" + Dynamic.Core.Runtime.SerializationUtility.ToXmlString(command));
        }
    }


    /// <remarks/>
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class WSCommandTypeParameter
    {

        private string nameField;

        private string valueField;

        /// <remarks/>
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
}
