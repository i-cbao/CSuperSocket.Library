using CSuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace TcpServerDemo
{
    public static class Extions2 {
        public static string ToHex(this byte[] bytes, uint index, uint length)
        {
            var values = new byte[length];
            Array.Copy(bytes, index, values, 0, values.Length);
            return System.Text.Encoding.UTF8.GetString(values);
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(values));
           
            
            return values.ToHex();
        }
        public static string ToHex(this byte[] bytes)
        {
            //性能损耗非常高
            //var strArray = bytes.Select(b => b.ToString("x2"));
            //string strHex = string.Concat(strArray);
            //return strHex;

            lock (bytes)
            {
                var sb = new StringBuilder();
                foreach (var b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }

        }

    }
    public class ReceiveFilter : IReceiveFilter<RequestInfo>
    {
        DefaultBeginEndMarkReceiveFilter defaultFilter = new DefaultBeginEndMarkReceiveFilter();

        public int LeftBufferSize
        {
            get;
        }
      

        public FilterState State
        {
            get;
        }

        public IReceiveFilter<RequestInfo> NextReceiveFilter
        {
            get; set;
        }


        public RequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {

            rest = length;
            var tag = readBuffer.ToHex((uint)offset, 1);
            if (tag != "7e")
            {
                tag = readBuffer.ToHex((uint)offset, 2);
            }
            //if(tag=="7e")
            //{
            //    var contextBuffer = new byte[length];
            //    Array.Copy(readBuffer,offset, contextBuffer, 0, contextBuffer.Length);
            //    return new RequestInfo(tag, contextBuffer);
            //}
           
            switch (tag)
            {
                case "7e":
                    NextReceiveFilter = defaultFilter;
                    break;
                case "7878":
                case "7979":
                    NextReceiveFilter = defaultFilter;
                    break;
            }
            return null;
        }

        public void Reset()
        {
          
        }
    }
}
