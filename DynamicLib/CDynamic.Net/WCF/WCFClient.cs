#if WCF_SUPPORTED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml.Linq;

namespace Dynamic.Net.WCF
{

    /// <summary>
    /// 通用的，使用BasicHttpBinding的服务客户端
    /// </summary>
    public class WCFClient : ClientBase<IWCFClient>, IWCFClient
    {
        private BasicHttpBinding innerBinding = null;
        public WCFClient(string address)
            : base(new BasicHttpBinding(), new EndpointAddress(address))
        {
            BasicHttpBinding basicHttpBinding = base.Endpoint.Binding as BasicHttpBinding;
            basicHttpBinding.MaxBufferPoolSize = 1024 * 1024 * 10;
            basicHttpBinding.MaxBufferSize = 1024 * 1024 * 10;
            basicHttpBinding.MaxReceivedMessageSize = 1024 * 1024 * 10;
            basicHttpBinding.ReaderQuotas.MaxArrayLength = 1024000;
            basicHttpBinding.ReaderQuotas.MaxBytesPerRead = 1024 * 1024 * 10;
            basicHttpBinding.ReaderQuotas.MaxDepth = 1024000;
            basicHttpBinding.ReaderQuotas.MaxStringContentLength = 1024 * 1024 * 10;
            basicHttpBinding.ReaderQuotas.MaxNameTableCharCount = 1024000;

            innerBinding = basicHttpBinding;
        }

        public TimeSpan SendTimeout
        {
            get { return innerBinding.SendTimeout; }
            set { innerBinding.SendTimeout = value; }
        }

        public TimeSpan ReceiveTimeout
        {
            get { return innerBinding.ReceiveTimeout; }
            set { innerBinding.ReceiveTimeout = value; }
        }

        public string Request(string action, string soapMessage)
        {
            XElement requestElement = XElement.Parse(soapMessage);

            Message requestMessage = ServiceHelper.CreateMessage(action, requestElement);

            Message responseMessage = Request(requestMessage);

            return ServiceHelper.MessageToString(responseMessage);
        }

        public void Invoke(string action, string soapMessage)
        {
            XElement requestElement = XElement.Parse(soapMessage);

            Message requestMessage = ServiceHelper.CreateMessage(action, requestElement);

            Invoke(requestMessage);
        }

        public String Get()
        {
            Message responseMessage = (this as IWCFClient).Get();

            return ServiceHelper.MessageToString(responseMessage);
        }

#region IWCFClient 成员

        public Message Request(Message request)
        {
            Message msg = base.Channel.Request(request);

            return msg;
        }

   
        public void Invoke(Message request)
        {
            base.Channel.Invoke(request);
        }

        Message IWCFClient.Get()
        {
            return base.Channel.Get();
        }

#endregion
    }


    [ServiceContract]
    public interface IWCFClient
    {
        [OperationContract(Action = "*", ReplyAction = "*")]
        Message Request(Message request);

        [OperationContract(Action = "*", ReplyAction = "*")]
        void Invoke(Message request);

        [OperationContract(Action = "*", ReplyAction = "*")]
        Message Get();
    }
}
#endif