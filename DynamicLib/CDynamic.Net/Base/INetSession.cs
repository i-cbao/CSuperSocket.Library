using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.Base
{
    public interface INetSession
    {
        event EventHandler SessionClosed;

        event EventHandler SessionStarted;

        INetApplication Application { get; }

        INetProtocol Protocol { get; }

        string SessionID { get; }

        INetServer Server { get; }

        Encoding Encoding { get; set; }

        TimeSpan Timeout { get; set; }

        DateTime ActiveTime { get; set; }


        /// <summary>
        /// 关闭会话
        /// </summary>
        bool Close();

        void Start();

        bool ReadBytes(Byte[] data, int start, int count);

        bool WriteBytes(Byte[] data, int start, int count);

        bool IsTimeout();
    }
}
