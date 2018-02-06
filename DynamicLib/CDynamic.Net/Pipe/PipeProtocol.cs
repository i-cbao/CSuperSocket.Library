using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.IO.Pipes;
using Dynamic.Core.Log;

namespace Dynamic.Net.Pipe
{
    public class PipeProtocol
    {
        protected ILogger Logger = LoggerManager.GetLogger("Pipe");

        private Stream readStream = null;
        private Stream writeStream = null;
        public PipeProtocol(Stream readStream, Stream writeStream)
        {
            this.readStream = readStream;
            this.writeStream = writeStream;
        }

        public void WriteData(byte[] data)
        {
            if (data != null && data.Length >0 )            
            {
              //  DateTime d1 = DateTime.Now;
                 Int32 len = data.Length;
                 byte[] lenBytes = BitConverter.GetBytes(len);

                byte[] sendBytes = new byte[lenBytes.Length + data.Length ];
                Buffer.BlockCopy( lenBytes,0,sendBytes, 0, lenBytes.Length );
                Buffer.BlockCopy( data, 0, sendBytes, lenBytes.Length , data.Length );
              //  Logger.Trace("耗时1：{0}", (DateTime.Now - d1).TotalMilliseconds);
             //   d1 = DateTime.Now;
                try
                {
                    IAsyncResult r = writeStream.BeginWrite(sendBytes, 0, sendBytes.Length,
                        new AsyncCallback((r1) =>
                        {
                            PipeStream ps = r1.AsyncState as PipeStream;
                            try
                            {
                                ps.EndWrite(r1);
                            }
                            catch { }
                        }), writeStream);
                   // Logger.Trace("耗时2:{0}", (DateTime.Now - d1).TotalMilliseconds);
                }
                catch { }
                
                
            }
        }

        public byte[] ReadData()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            byte[] data = null;
            Byte[] tempBytes = BitConverter.GetBytes((Int32)0);
            int readCount = 0;
            try
            {
                readCount = readStream.Read(tempBytes, 0, tempBytes.Length);
            }
            catch
            {
                return data;
            }
            if (readCount == tempBytes.Length)
            {
                int len = BitConverter.ToInt32(tempBytes,0);
                //100 MB
                if (len <= 1024 * 1024 * 100)
                {
                    data = new byte[len];
                    try
                    {
                        readStream.Read(data, 0, data.Length);
                    }
                    catch
                    {
                        data = null;
                    }
                }
                else
                {
                    byte[] skipBytes = new byte[255];
                    int pc = 0;
                    while (pc < len)
                    {
                        int rc = 255;
                        if (pc + rc > len)
                        {
                            rc = len - pc;
                        }
                        try
                        {
                            rc = readStream.Read(skipBytes, 0, rc);
                        }
                        catch
                        {
                            break;
                        }
                        pc += rc;
                        if (rc <= 0)
                        {
                            break;
                        }
                    }
                }
            }

            return data;

        }

        

    }
}
