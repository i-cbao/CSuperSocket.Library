using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    public class WSBinaryCommandType : WSCommandTypeBase
    {
       // public Guid RequestID { get; set; }
        public WSBinaryCommandTypeParameter[] Parameters { get; set; }


        public override string ParserID
        {
            get { return "Binary"; }
        }

        public override MessageContentType TransferEncoder
        {
            get { return MessageContentType.Binary; }
        }

        public override bool CanRead(MessageReceivedEventArgs args, out WSCommandTypeBase command)
        {
            command = null;
            if (args.ContentType == TransferEncoder)
            {
                if (args.Data != null && args.Data.Length >= 2 &&
                    args.Data[0] == 0 && args.Data[1] == 0)
                {
                    command = BinaryCommandTypeSerializer.ToCommandType(args.Data);
                    return true;
                }
            }

            return false;
        }

        public override void SetReplyCommand(MessageReceivedEventArgs args,WSCommandTypeBase command)
        {
            args.ResponseData = BinaryCommandTypeSerializer.ToBinary(command as WSBinaryCommandType);
        }

        public override WSCommandTypeBase Create()
        {
            return new WSBinaryCommandType();
        }

        public override void CreateParameters(int count)
        {
            Parameters = new WSBinaryCommandTypeParameter[count];
        }

        public override void AddParameter(string name, int index)
        {
            Parameters[index] = new WSBinaryCommandTypeParameter() { Name = name, Value = null };
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
            if (command is WSBinaryCommandType)
            {
                return (command as WSBinaryCommandType).RequestID == this.RequestID;
            }

            return false;
        }

        public override void SetCommandParameter(string name, string value)
        {
            WSBinaryCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                throw new NotSupportedException();
            }

            par.Value = Encoding.UTF8.GetBytes(value ?? "");
        }

        public override void SetCommandParameter(string name, byte[] value)
        {
            WSBinaryCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                throw new NotSupportedException();
            }

            par.Value = value;

        }

        public override void SetCommandParameter(string name, int value)
        {
            WSBinaryCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                throw new NotSupportedException();
            }

            par.Value = convertToBytes(value);

        }

        public override void SetCommandParameter(string name, long value)
        {
            WSBinaryCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                throw new NotSupportedException();
            }

            par.Value = convertToBytes(value);
        }

        public override void SetCommandParameter(string name, decimal value)
        {
            WSBinaryCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                throw new NotSupportedException();
            }

            par.Value = convertToBytes(value);
        }

        public override void SetCommandParameter(string name, bool value)
        {
            WSBinaryCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                throw new NotSupportedException();
            }

            par.Value = convertToBytes(value);
        }


        public override void SetCommandParameter(string name, object value)
        {
            string xml = Dynamic.Core.Runtime.SerializationUtility.ToXmlString(value);
            SetCommandParameter(name, xml);
        }

        private byte[] convertToBytes(int value)
        {
            byte[] result = new byte[4];

            result[0] = (byte)value;
            result[1] = (byte)(value >> 8);
            result[2] = (byte)(value >> 0x10);
            result[3] = (byte)(value >> 0x18);

            return result;
        }

        private byte[] convertToBytes(ulong value)
        {
            byte[] result = new byte[8];

            result[0] = (byte)value;
            result[1] = (byte)(value >> 8);
            result[2] = (byte)(value >> 0x10);
            result[3] = (byte)(value >> 0x18);
            result[4] = (byte)(value >> 0x20);
            result[5] = (byte)(value >> 40);
            result[6] = (byte)(value >> 0x30);
            result[7] = (byte)(value >> 0x38);

            return result;
        }

        private byte[] convertToBytes(decimal value)
        {
            byte[] result = new byte[16];

            int[] bits = decimal.GetBits(value);

            byte[] tmp = convertToBytes(bits[0]);
            Array.Copy(tmp, 0, result, 0, 4);

            tmp = convertToBytes(bits[1]);
            Array.Copy(tmp, 0, result, 4, 4);

            tmp = convertToBytes(bits[2]);
            Array.Copy(tmp, 0, result, 8, 4);

            tmp = convertToBytes(bits[3]);
            Array.Copy(tmp, 0, result, 12, 4);

            return result;
        }

        private byte[] convertToBytes(Boolean value)
        {
            byte[] result = new byte[1];
            result[0] = value ? ((byte)1) : ((byte)0);

            return result;

        }



        public override string GetCommandParameterStringValue(string name)
        {
            byte[] parValue = GetCommandParameterBinaryValue(name);
            if (parValue == null)
            {
                return "";
            }

            return Encoding.UTF8.GetString(parValue);
        }

        public override byte[] GetCommandParameterBinaryValue(string name)
        {
            WSBinaryCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                return null;
            }

            return par.Value;
        }

        public override int GetCommandParameterIntValue(string name)
        {
            WSBinaryCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                return int.MinValue;
            }

            return convertToInt(par.Value);
        }

        public override long GetCommandParameterLongValue(string name)
        {
            WSBinaryCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                return long.MinValue;
            }

            return convertToLong(par.Value);
        }

        public override decimal GetCommandParameterDecimalValue(string name)
        {
            WSBinaryCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                return decimal.MinValue;
            }

            return convertToDecimal(par.Value);
        }

        public override Boolean GetCommandParameterBooleanValue(string name)
        {
            WSBinaryCommandTypeParameter par = this.Parameters.FirstOrDefault(x => (x.Name ?? "").Equals(name, StringComparison.OrdinalIgnoreCase));
            if (par == null)
            {
                return false;
            }

            return convertToBoolean(par.Value);
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

        private int convertToInt(byte[] value)
        {
            return (((value[0] | (value[1] << 8)) | (value[2] << 0x10)) | (value[3] << 0x18));

        }

        private long convertToLong(byte[] value)
        {
            uint num = (uint)(((value[0] | (value[1] << 8)) | (value[2] << 0x10)) | (value[3] << 0x18));
            uint num2 = (uint)(((value[4] | (value[5] << 8)) | (value[6] << 0x10)) | (value[7] << 0x18));
            return (long)((num2 << 0x20) | num);
        }

        private decimal convertToDecimal(byte[] value)
        {
            int[] bits = new int[4];

            byte[] tmps = new byte[4];
            Array.Copy(value, 0, tmps, 0, 4);
            bits[0] = convertToInt(tmps);

            Array.Copy(value, 4, tmps, 0, 4);
            bits[1] = convertToInt(tmps);

            Array.Copy(value, 8, tmps, 0, 4);
            bits[2] = convertToInt(tmps);

            Array.Copy(value, 12, tmps, 0, 4);
            bits[3] = convertToInt(tmps);

            return new decimal(bits);
        }

        private bool convertToBoolean(byte[] value)
        {
            return (value[0] != 0);
        }


        public override byte[] ToBinary(WSCommandTypeBase command)
        {
            return BinaryCommandTypeSerializer.ToBinary(command as WSBinaryCommandType);
        }
    }


    public class WSBinaryCommandTypeParameter
    {
        public string Name { get; set; }

        public byte[] Value { get; set; }
    }
}
