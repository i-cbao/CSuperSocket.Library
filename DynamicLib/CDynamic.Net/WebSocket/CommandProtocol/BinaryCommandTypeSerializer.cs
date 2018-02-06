using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dynamic.Net.WebSocket.CommandProtocol
{
    public static class BinaryCommandTypeSerializer
    {
        public static WSBinaryCommandType ToCommandType(byte[] commandData)
        {
            MemoryStream ms = new MemoryStream(commandData);
            ms.Flush();

            return ToCommandType(ms);
        }

        public static WSBinaryCommandType ToCommandType(Stream stream)
        {
            stream.Position = 0;
            BinaryReader br = new BinaryReader(stream);
            if (stream.Length <2 ||  br.ReadByte() != 0 || br.ReadByte() != 0)
            {
                return null;
            }

            WSBinaryCommandType commandObj = new WSBinaryCommandType();

            //Request ID
            byte[] requestId = br.ReadBytes(16);
            commandObj.RequestID = new Guid(requestId);

            //OccurTime
            long ticks = br.ReadInt64();
            commandObj.OccurTime = new DateTime(ticks);

            //Name
            commandObj.CommandName = readString(br);

            //Type
            commandObj.CommandType = readString(br);

            //IsOver
            byte isOver = br.ReadByte();
            commandObj.IsOver = (isOver == 0 ? false : true);

            //Parameter List
            //Count
            int parCount = br.ReadInt32();
            if (parCount > 0)
            {
                commandObj.Parameters = new WSBinaryCommandTypeParameter[parCount];
                for (int i = 0; i < parCount; i++)
                {
                    commandObj.Parameters[i] = new WSBinaryCommandTypeParameter();
                    commandObj.Parameters[i].Name = readString(br);
                    int valLen = br.ReadInt32();
                    if (valLen > 0)
                    {
                        commandObj.Parameters[i].Value = br.ReadBytes(valLen);
                    }
                }
            }

            return commandObj;
        }

        private static String readString(BinaryReader br)
        {
            int len = br.ReadInt32();
            if (len == 0)
                return "";

            byte[] strData = br.ReadBytes(len);
            return Encoding.UTF8.GetString(strData);
        }


        public static Stream ToStream(WSBinaryCommandType commandObject)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            //固定标头
            bw.Write((byte)0);
            bw.Write((byte)0);

            //Request Guid
            bw.Write(commandObject.RequestID.ToByteArray());

            //OccurTime
            bw.Write(commandObject.OccurTime.Ticks);

            //Name
            writeString(bw, commandObject.CommandName);

            //Type
            writeString(bw, commandObject.CommandType);
            
            //IsOver
            bw.Write((byte)(commandObject.IsOver ? 1 : 0));

            //Parameter List
            //Count
            bw.Write(commandObject.Parameters == null ? (int)0 : commandObject.Parameters.Length);

            //Parameter
            if (commandObject.Parameters != null && commandObject.Parameters.Any())
            {
                foreach (WSBinaryCommandTypeParameter p in commandObject.Parameters)
                {
                    writeString(bw, p.Name);
                    bw.Write(p.Value == null ? (int)0 : p.Value.Length);
                    if (p.Value != null && p.Value.Length > 0)
                    {
                        bw.Write(p.Value);
                    }
                }
            }

            ms.Flush();

            return ms;
        }

        private static void writeString(BinaryWriter bw, string content)
        {
            byte[] contentData = null;
            if (!String.IsNullOrEmpty(content))
            {
                contentData = Encoding.UTF8.GetBytes(content);
            }
            bw.Write(contentData == null ? (int)0 : contentData.Length);
            if (contentData != null && contentData.Length > 0)
            {
                bw.Write(contentData);
            }
        }

        public static byte[] ToBinary(WSBinaryCommandType commandObject)
        {
            MemoryStream ms = ToStream(commandObject) as MemoryStream;
            ms.Position = 0;
            BinaryReader br = new BinaryReader(ms);
            byte[] data = br.ReadBytes((int)ms.Length);
            br.Close();

            return data;
        }

    }
}
