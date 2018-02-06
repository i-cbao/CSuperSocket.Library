using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamic.Net.WebSocket.CommandProtocol;
using System.IO;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    /// <summary>
    /// 自定义命令接口基类
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        protected List<String> commandParameters = null;
        public String Name { get; protected set; }
        public String Type { get; protected set; }

      //  public WSCommandTypeBase Command { get; protected set; }

        protected CommandBase(string name, string type, params string[] parameters)
        {
            commandParameters = new List<string>();
            if (parameters != null && parameters.Any())
            {
                commandParameters.AddRange(parameters);
            }

            this.Name = name;
            this.Type = type;
        }

        protected abstract void SetCommandParameters(WSCommandTypeBase command);

        //public virtual WSCommandTypeBase ToCommand(IWebSocketCommandFactory commandFactory)
        //{
        //    WSCommandTypeBase command = GetEmptyCommand(commandFactory);
        //    SetCommandParameters(command);
        //    return command;
        //}

        public virtual void ToCommand(WSCommandTypeBase command)
        {
            GetEmptyCommand(command);
            SetCommandParameters(command);
        }

        public abstract ICommand Parse(WSCommandTypeBase command);

        //protected byte[] GetCommandValue(string name)
        //{
          
        //}


        //public virtual WSBinaryCommandType Create()
        //{
        //    Command = CreateCommand();
        //    return Command;
        //}

        public abstract void LoadCommand(WSCommandTypeBase command);

        public virtual void GetEmptyCommand(WSCommandTypeBase command)
        {
            command.CommandName = this.Name;
            command.CommandType = this.Type;

            if (commandParameters.Count > 0)
            {
                command.CreateParameters(commandParameters.Count);
                for (int i = 0; i < commandParameters.Count; i++)
                {
                    command.AddParameter(commandParameters[i], i);
                }
            }
        }

        //protected virtual void SetCommandParameter(WSBinaryCommandType command, string name, string value)
        //{
        //    WSBinaryCommandTypeParameter par = command.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
        //    if (par == null)
        //    {
        //        throw new NotSupportedException();
        //    }

        //    par.Value = Encoding.UTF8.GetBytes(value ?? "");
        //}

        //protected virtual void SetCommandParameter(WSBinaryCommandType command, string name, byte[] value)
        //{
        //    WSBinaryCommandTypeParameter par = command.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
        //    if (par == null)
        //    {
        //        throw new NotSupportedException();
        //    }

        //    par.Value = value;

        //}

        //protected virtual void SetCommandParameter(WSBinaryCommandType command, string name, int value)
        //{
        //    WSBinaryCommandTypeParameter par = command.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
        //    if (par == null)
        //    {
        //        throw new NotSupportedException();
        //    }

        //    par.Value = convertToBytes(value);

        //}

        //protected virtual void SetCommandParameter(WSBinaryCommandType command, string name, long value)
        //{
        //    WSBinaryCommandTypeParameter par = command.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
        //    if (par == null)
        //    {
        //        throw new NotSupportedException();
        //    }

        //    par.Value = convertToBytes(value);
        //}

        //protected virtual void SetCommandParameter(WSBinaryCommandType command, string name, decimal value)
        //{
        //    WSBinaryCommandTypeParameter par = command.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
        //    if (par == null)
        //    {
        //        throw new NotSupportedException();
        //    }

        //    par.Value = convertToBytes(value);
        //}

        //protected virtual void SetCommandParameter(WSBinaryCommandType command, string name, bool value)
        //{
        //    WSBinaryCommandTypeParameter par = command.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
        //    if (par == null)
        //    {
        //        throw new NotSupportedException();
        //    }

        //    par.Value = convertToBytes(value);
        //}


        //protected virtual void SetCommandParameter(WSBinaryCommandType command, string name, object value)
        //{
        //    string xml = Dynamic.Core.Runtime.SerializationUtility.ToXmlString(value);
        //    SetCommandParameter(command, name, xml);
        //}

        //private byte[] convertToBytes(int value)
        //{
        //    byte[] result = new byte[4];

        //    result[0] = (byte) value;
        //    result[1] = (byte) (value >> 8);
        //    result[2] = (byte) (value >> 0x10);
        //    result[3] = (byte) (value >> 0x18);

        //    return result;
        //}

        //private byte[] convertToBytes(ulong value)
        //{
        //    byte[] result = new byte[8];

        //    result[0] = (byte)value;
        //    result[1] = (byte)(value >> 8);
        //    result[2] = (byte)(value >> 0x10);
        //    result[3] = (byte)(value >> 0x18);
        //    result[4] = (byte)(value >> 0x20);
        //    result[5] = (byte)(value >> 40);
        //    result[6] = (byte)(value >> 0x30);
        //    result[7] = (byte)(value >> 0x38);

        //    return result;
        //}

        //private byte[] convertToBytes(decimal value)
        //{
        //    byte[] result = new byte[16];

        //    int[] bits = decimal.GetBits(value);

        //    byte[] tmp = convertToBytes(bits[0]);
        //    Array.Copy(tmp, 0, result, 0, 4);

        //    tmp = convertToBytes(bits[1]);
        //    Array.Copy(tmp, 0, result, 4, 4);

        //    tmp = convertToBytes(bits[2]);
        //    Array.Copy(tmp, 0, result, 8, 4);

        //    tmp = convertToBytes(bits[3]);
        //    Array.Copy(tmp, 0, result, 12, 4);

        //    return result;
        //}

        //private byte[] convertToBytes(Boolean value)
        //{
        //    byte[] result = new byte[1];
        //    result[0] = value ? ((byte)1) : ((byte)0);

        //    return result;

        //}

        //protected virtual string GetCommandParameterStringValue(WSBinaryCommandType command, string name)
        //{
        //    byte[] parValue = GetCommandParameterBinaryValue(command, name);
        //    if (parValue == null)
        //    {
        //        return "";
        //    }

        //    return Encoding.UTF8.GetString(parValue);
        //}

        //protected virtual byte[] GetCommandParameterBinaryValue(WSBinaryCommandType command, string name)
        //{
        //    WSBinaryCommandTypeParameter par = command.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
        //    if (par == null)
        //    {
        //        return null;
        //    }

        //    return par.Value;
        //}



        //protected virtual int GetCommandParameterIntValue(WSBinaryCommandType command, string name)
        //{
        //    WSBinaryCommandTypeParameter par = command.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
        //    if (par == null)
        //    {
        //        return int.MinValue;
        //    }

        //    return convertToInt(par.Value);
        //}

        //protected virtual long GetCommandParameterLongValue(WSBinaryCommandType command, string name)
        //{
        //    WSBinaryCommandTypeParameter par = command.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
        //    if (par == null)
        //    {
        //        return long.MinValue;
        //    }

        //    return convertToLong(par.Value);
        //}

        //protected virtual decimal GetCommandParameterDecimalValue(WSBinaryCommandType command, string name)
        //{
        //    WSBinaryCommandTypeParameter par = command.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
        //    if (par == null)
        //    {
        //        return decimal.MinValue;
        //    }

        //    return convertToDecimal(par.Value);
        //}

        //protected virtual Boolean GetCommandParameterBooleanValue(WSBinaryCommandType command, string name)
        //{
        //    WSBinaryCommandTypeParameter par = command.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
        //    if (par == null)
        //    {
        //        return false;
        //    }

        //    return convertToBoolean(par.Value);
        //}

        //protected virtual T GetCommandParameterObjectValue<T>(WSBinaryCommandType command, string name)
        //{
        //    string xml = GetCommandParameterStringValue(command, name);
        //    if (String.IsNullOrEmpty(xml))
        //    {
        //        return default(T);
        //    }

        //    return Dynamic.Core.Runtime.SerializationUtility.ToObject<T>(xml);
        //}

        //private int convertToInt(byte[] value)
        //{
        //    return (((value[0] | (value[1] << 8)) | (value[2] << 0x10)) | (value[3] << 0x18));

        //}

        //private long convertToLong(byte[] value)
        //{
        //    uint num = (uint)(((value[0] | (value[1] << 8)) | (value[2] << 0x10)) | (value[3] << 0x18));
        //    uint num2 = (uint)(((value[4] | (value[5] << 8)) | (value[6] << 0x10)) | (value[7] << 0x18));
        //    return (long)((num2 << 0x20) | num);
        //}

        //private decimal convertToDecimal(byte[] value)
        //{
        //    int[] bits = new int[4];

        //    byte[] tmps = new byte[4];
        //    Array.Copy(value, 0, tmps, 0, 4);
        //    bits[0] = convertToInt(tmps);

        //    Array.Copy(value, 4, tmps, 0, 4);
        //    bits[1] = convertToInt(tmps);

        //    Array.Copy(value, 8, tmps, 0, 4);
        //    bits[2] = convertToInt(tmps);

        //    Array.Copy(value, 12, tmps, 0, 4);
        //    bits[3] = convertToInt(tmps);

        //    return new decimal(bits);
        //}

        //private bool convertToBoolean(byte[] value)
        //{
        //    return (value[0] != 0);
        //}

        #region ICommand 成员

        public virtual bool CanExecute(Dynamic.Net.WebSocket.CommandProtocol.WSCommandTypeBase command)
        {
            if (command == null)
                return false;

            if (command.CommandName == this.Name && command.CommandType == this.Type &&
                command.ParameterCount  == this.commandParameters.Count )
            {
                if (this.commandParameters.Count == 0)
                    return true;

                bool isSuccess = true;
                this.commandParameters.All(x =>
                {
                    if (!command.HasParameter(x))
                    {
                        isSuccess = false;
                        return false;
                    }
                    return true;
                });

                return isSuccess;
            }

            return false;
        }

        public abstract Dynamic.Net.WebSocket.CommandProtocol.ICommand Execute(Dynamic.Net.WebSocket.CommandProtocol.WSCommandTypeBase command, ExecuteCommandContext context);

        #endregion
    }
}
