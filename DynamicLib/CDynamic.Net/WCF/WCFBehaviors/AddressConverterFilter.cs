#if WCF_SUPPORTED
using System;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using Dynamic.Core.Log;

namespace Dynamic.Net.WCF
{
    public class AddressConverterFilter : MessageFilter
    {
        MessageFilter filter = null;
        private Type serviceType = null;
        private ILogger receiveLogger = null;
        private EndpointAddress address = null;

        public AddressConverterFilter(MessageFilter messageFilter, Type serviceType, EndpointAddress address)
        {
            filter = messageFilter;
            this.serviceType = serviceType;
            this.address = address;

            receiveLogger = LoggerManager.GetLogger("AddressConverter");
        }


        public override bool Match(System.ServiceModel.Channels.Message message)
        {
            if (String.IsNullOrEmpty(message.Headers.Action))
            {
                if (message.Headers.To != null)
                {
                    if (!String.IsNullOrEmpty(message.Headers.To.Query))
                    {
                        if (message.Headers.To.Query.StartsWith("?op=", StringComparison.OrdinalIgnoreCase))
                        {
                            message.Headers.Action = message.Headers.To.Query.Substring(4).Trim();
                            receiveLogger.Info("调整Action:{0} 请求地址:{1}", message.Headers.Action, message.Headers.To.ToString());
                            
                        }
                    }
                    else
                    {
                        string requestUrl = message.Headers.To.ToString();
                        string serviceUrl = address.Uri.ToString();
                        if (!serviceUrl.Equals(requestUrl, StringComparison.OrdinalIgnoreCase) && requestUrl.Length > serviceUrl.Length &&
                            requestUrl.StartsWith(serviceUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            string action = requestUrl.Substring(serviceUrl.Length + 1);
                            message.Headers.Action = action;
                            message.Headers.To = new Uri(serviceUrl);
                            receiveLogger.Info("调整Action:{0} 请求地址:{1}", message.Headers.Action, requestUrl);
                        }
                    }
                }
            }

            if (message.Headers.To == null)
            {
                message.Headers.To = this.address.Uri;
            }

            bool isMatch = filter.Match(message);
            
            return isMatch;
        }

        public override bool Match(System.ServiceModel.Channels.MessageBuffer buffer)
        {

            if (filter != null)
            {
                return filter.Match(buffer);
            }
            return true;
        }
    }
}
#endif