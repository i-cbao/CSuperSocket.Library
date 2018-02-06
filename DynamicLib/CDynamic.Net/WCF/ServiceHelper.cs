#if WCF_SUPPORTED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace Dynamic.Net.WCF
{
    public static class ServiceHelper
    {
        /// <summary>
        /// 创建一个宿主，默认使用HttpBasicBinding，兼容传统asmx服务
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="interfaceType"></param>
        /// <param name="baseAddress"></param>
        /// <returns></returns>
        public static ServiceHost CreateServiceHost(Type serviceType, Type interfaceType, string baseAddress)
        {
            ServiceHost serviceHost = new ServiceHost(serviceType, new Uri(baseAddress));

            ServiceDebugBehavior debugBehavior = serviceHost.Description.Behaviors.FirstOrDefault(x => x is ServiceDebugBehavior) as ServiceDebugBehavior;
            if (debugBehavior == null)
            {
                debugBehavior = new ServiceDebugBehavior();
                serviceHost.Description.Behaviors.Add(debugBehavior);
            }
            debugBehavior.IncludeExceptionDetailInFaults = true;

            ServiceBehaviorAttribute serviceBehavior = serviceHost.Description.Behaviors.FirstOrDefault(x => x is ServiceBehaviorAttribute) as ServiceBehaviorAttribute;
            if (serviceBehavior == null)
            {
                serviceBehavior = new ServiceBehaviorAttribute();
                serviceHost.Description.Behaviors.Add(serviceBehavior);
            }
            serviceBehavior.UseSynchronizationContext = false;
            serviceBehavior.ConcurrencyMode = ConcurrencyMode.Multiple;

            BasicHttpBinding basicHttpBinding = new BasicHttpBinding();
            basicHttpBinding.MaxBufferPoolSize = 1024 * 1024 * 10;
            basicHttpBinding.MaxBufferSize = 1024 * 1024 * 10;
            basicHttpBinding.MaxReceivedMessageSize = 1024 * 1024 * 10;
            basicHttpBinding.ReaderQuotas.MaxArrayLength = 1024000;
            basicHttpBinding.ReaderQuotas.MaxBytesPerRead = 1024 * 1024 * 10;
            basicHttpBinding.ReaderQuotas.MaxDepth = 1024000;
            basicHttpBinding.ReaderQuotas.MaxStringContentLength = 1024 * 1024 * 10;
            basicHttpBinding.ReaderQuotas.MaxNameTableCharCount = 1024000;

            serviceHost.AddServiceEndpoint(interfaceType, basicHttpBinding, baseAddress);

            //线程支持数
            ServiceThrottlingBehavior stb = serviceHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            if (stb == null)
            {
                stb = new ServiceThrottlingBehavior();
                serviceHost.Description.Behaviors.Add(stb);
            }
            stb.MaxConcurrentCalls = 1000;
            stb.MaxConcurrentInstances = 1000;
            stb.MaxConcurrentSessions = 1000;

            //支持 ?op=ActionName调用方式
            serviceHost.Description.Behaviors.Add(new AdjustActionNameServiceBehavior());

            ServiceMetadataBehavior smb = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (smb == null)
            {
                smb = new ServiceMetadataBehavior();
                serviceHost.Description.Behaviors.Add(smb);
            }

            smb.HttpGetEnabled = true;


            return serviceHost;

        }

        /// <summary>
        /// 根据SOAP消息的XML创建一个消息
        /// </summary>
        /// <param name="version">SOAP消息版本</param>
        /// <param name="action">活动方法名称</param>
        /// <param name="envelop">SOAP消息XML</param>
        /// <returns></returns>
        public static Message CreateMessage(string action, XElement envelop)
        {
            MemoryStream envelopStream = new MemoryStream();
            StreamWriter sw = new StreamWriter(envelopStream);
            sw.Write( envelop.ToString());
            sw.Flush();
            envelopStream.Flush();
            envelopStream.Position = 0;

            MessageVersion version = MessageVersion.None;
            string ns = envelop.Name.NamespaceName;

            if (ns == "http://schemas.xmlsoap.org/soap/envelope/")
            {
                version = MessageVersion.Soap11;
            }
            else if (ns == " http://www.w3.org/2003/05/soap-envelope")
            {
                version = MessageVersion.Soap12;
            }
           

            XmlReader envelopReader = XmlReader.Create(envelopStream);

            Message message = Message.CreateMessage(envelopReader, int.MaxValue, version);

            if (!String.IsNullOrEmpty(action))
            {
                message.Headers.Action = action;
            }

            return message;
        }


        public static String MessageToString(Message response)
        {
            MemoryStream stream = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(stream);

            response.WriteMessage(writer);
            writer.Flush();
            stream.Flush();

            stream.Position = 0;

            StreamReader sr = new StreamReader(stream);

            string responseXml = sr.ReadToEnd();

            return responseXml;
        }
    }
}
#endif
