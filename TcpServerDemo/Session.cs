using CSuperSocket.SocketBase;
using CSuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace TcpServerDemo
{
    public class RequestInfo : BinaryRequestInfo
    {
        public RequestInfo(string key, byte[] body)
            : base(key, body)
        {

        }
    }
    public class Session : AppSession<Session, RequestInfo>
    {
        /// <summary>
        /// 设备号
        /// </summary>
        public string DeviceCode;

        /// <summary>
        /// 物联网卡ICCID
        /// </summary>
        public string IccId;

        /// <summary>
        /// 车辆配置
        /// </summary>
      //  public CustomConfig CustomConfig;

        /// <summary>
        /// 1：中胜（用于电子围栏）
        /// </summary>
        public int Flag = 0;

        /// <summary>
        /// 协议解析器
        /// </summary>
       // public ProtocolAnalysis PA;
      
        
        protected override void HandleUnknownRequest(RequestInfo requestInfo)
        {
            Logger.Warn(requestInfo.Body.ToString());
        }

        protected override void HandleException(Exception e)
        {
            Logger.Error(e.ToString());
        }

        public void Send(byte[] bytes)
        {
            Send(bytes, 0, bytes.Length);
        }
    }
}
