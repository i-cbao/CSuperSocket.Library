using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;

namespace Dynamic.Net.Pipe
{
    public delegate void ReceivedDataHandler(object sender, byte[] data);

    public abstract class PipeBase
    {
        
        protected PipeStream readStream = null;
        protected PipeStream  writeStream = null;

        protected PipeProtocol protocol = null;

        public event ReceivedDataHandler ReceivedData;

        public String ClientReadHandler { get; protected set; }
        public String ClientWriteHandler { get; protected set; }


        public virtual void Start()
        {
            readStream = CreateReadStream();
            writeStream = CreateWriteStream();
            protocol = new PipeProtocol(readStream, writeStream);

            StartReceived();
        }

        protected abstract PipeStream CreateReadStream();
        protected abstract PipeStream CreateWriteStream();
       

        public virtual void Stop()
        {
            stopWait = new System.Threading.ManualResetEvent(false);
            IsReceiving = false;
            stopWait.WaitOne(TimeSpan.FromSeconds(5));
            if (receiveThread != null && (receiveThread.ThreadState & ThreadState.Running) == ThreadState.Running)
            {
                try
                {
                    receiveThread.Abort();
                }
                catch { }
            }
            if (writeStream != null)
            {
                writeStream.Dispose();
            }
            if (readStream != null)
            {
                readStream.Dispose();
            }
            protocol = null;
            
        }


        public void SendData(byte[] data)
        {
            if (protocol != null)
            {
                protocol.WriteData(data);
            }
        }

        protected virtual void OnReceivedData(byte[] data)
        {
            if (ReceivedData != null)
            {
                ReceivedData(this, data);
            }
        }

        protected bool IsReceiving = false;
        protected System.Threading.ManualResetEvent stopWait = null;
        protected Thread receiveThread = null;
        protected virtual void StartReceived()
        {
            if (IsReceiving)
            {
                return;
            }
            IsReceiving = true;
            receiveThread = new Thread(new ThreadStart(ReceivedProc));
            receiveThread.IsBackground = true;
            receiveThread.Name = "Pipe Read";
            receiveThread.Start();
            
        }

        protected virtual void ReceivedProc()
        {
            try
            {
                while (IsReceiving)
                {
                    if (protocol != null)
                    {
                        byte[] data = protocol.ReadData();
                        if (data != null)
                        {
                            OnReceivedData(data);
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                IsReceiving = false;
                if (stopWait != null)
                {
                    stopWait.Set();
                }
            }

        }
    }
}
