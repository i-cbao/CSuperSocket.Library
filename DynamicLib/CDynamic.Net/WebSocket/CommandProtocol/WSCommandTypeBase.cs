using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    [Serializable]
    public abstract class WSCommandTypeBase : ICommandParser
    {
        private string commandTypeField;

        private System.DateTime occurTimeField;

        private string commandNameField;

        private Guid requestIDField;

        /// <remarks/>
        public string CommandType
        {
            get
            {
                return this.commandTypeField;
            }
            set
            {
                this.commandTypeField = value;
            }
        }

        /// <remarks/>
        public System.DateTime OccurTime
        {
            get
            {
                return this.occurTimeField;
            }
            set
            {
                this.occurTimeField = value;
            }
        }

        /// <remarks/>
        public string CommandName
        {
            get
            {
                return this.commandNameField;
            }
            set
            {
                this.commandNameField = value;
            }
        }

        /// <summary>
        /// 是否是结束命令（当命令用于反馈进度时，进度完成后应将此属性设置为true）
        /// </summary>
        private Boolean isOver = true;
        public Boolean IsOver
        {
            get { return isOver; }
            set { isOver = value; }
        }

        public abstract String ParserID { get; }

        public abstract MessageContentType TransferEncoder { get; }

        public abstract bool CanRead(MessageReceivedEventArgs args, out WSCommandTypeBase command);

        public abstract void SetReplyCommand(MessageReceivedEventArgs args, WSCommandTypeBase command);

        public abstract WSCommandTypeBase Create();

        public Guid RequestID
        {
            get { return requestIDField; }
            set { requestIDField = value; }
        }

        public abstract bool IsPairCommand(WSCommandTypeBase command);

        public abstract void CreateParameters(int count);

        public abstract void AddParameter(string name, int index);

        public abstract int ParameterCount { get; }

        public abstract bool HasParameter(string name);

        public abstract void SetCommandParameter(string name, string value);

        public abstract void SetCommandParameter(string name, byte[] value);

        public abstract void SetCommandParameter(string name, int value);

        public abstract void SetCommandParameter(string name, long value);

        public abstract void SetCommandParameter(string name, decimal value);

        public abstract void SetCommandParameter(string name, bool value);

        public abstract void SetCommandParameter(string name, object value);

        public abstract string GetCommandParameterStringValue(string name);

        public abstract byte[] GetCommandParameterBinaryValue(string name);

        public abstract int GetCommandParameterIntValue(string name);

        public abstract long GetCommandParameterLongValue(string name);

        public abstract decimal GetCommandParameterDecimalValue(string name);

        public abstract Boolean GetCommandParameterBooleanValue(string name);

        public abstract T GetCommandParameterObjectValue<T>(string name);


        public abstract Byte[] ToBinary(WSCommandTypeBase command);

        public Byte[] ToBinary()
        {
            return ToBinary(this);
        }

    }
}
