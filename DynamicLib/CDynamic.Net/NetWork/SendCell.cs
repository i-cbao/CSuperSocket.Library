using System;
using System.Collections.Generic;
using System.Text;

namespace Dynamic.Net.KcpSharp.NetWork
{

    [Serializable]
    public class SendCell 
    {
        private UInt16 _messageID;
        /// <summary>
        /// 获取消息ID
        /// </summary>
        public UInt16 MessageID
        {
            get { return _messageID; }
        }

        private byte[] _data;
        /// <summary>
        /// 获取消息数据对象
        /// </summary>
        public byte[] Data
        {
            get { return _data; }
        }

        public SendCell() { }

        public SendCell(
            UInt16 messageID,
            byte[] data)
        {
            _messageID = messageID;
            _data = data;
        }
        /// <summary>
        /// 将数据包转为Byte数组
        /// </summary>
        /// <returns></returns>
        public byte[] ToBuffer()
        {
            byte[] id = BitConverter.GetBytes(MessageID);

            byte[] buffer = new byte[_data.Length + id.Length];
            Buffer.BlockCopy(id, 0, buffer, 0, id.Length);
            Buffer.BlockCopy(_data, 0, buffer, id.Length, _data.Length);
            return buffer;
        }
        /// <summary>
        /// 从Byte数据获取数据包
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static SendCell FromBuffer(byte[] buffer)
        {
            try
            {
                SendCell sc = new SendCell();
                sc._messageID = BitConverter.ToUInt16(buffer, 0);
               byte[] dataBuffer = new byte[buffer.Length - 2]; 
                Buffer.BlockCopy(buffer, 2, dataBuffer, 0, buffer.Length - 2);
                sc._data = dataBuffer;
                return sc;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
