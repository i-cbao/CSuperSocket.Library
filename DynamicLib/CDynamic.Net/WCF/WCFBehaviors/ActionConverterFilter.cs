#if WCF_SUPPORTED
using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using Dynamic.Core.Log;

namespace Dynamic.Net.WCF
{
    public class ActionConverterFilter : XPathMessageFilter // MessageFilter
    {
        MessageFilter filter = null;
        private Type serviceType = null;
        private ILogger receiveLogger = null;
        private EndpointAddress address = null;

        public ActionConverterFilter(MessageFilter messageFilter, Type serviceType, EndpointAddress address)
        {
            filter = messageFilter;
            this.serviceType = serviceType;
            this.address = address;

            receiveLogger = LoggerManager.GetLogger("AddressConverter");
        }


        public override bool Match(System.ServiceModel.Channels.Message message)
        {

            //wHaibo 2013-05-14 Action再调整
            ActionMessageFilter actionFilter = filter as ActionMessageFilter;
            
            if (actionFilter != null)
            {
                if (!actionFilter.Actions.Contains(message.Headers.Action) && message.Headers.Action != null )
                {
                    string oldAction =message.Headers.Action;
                    string actionName = actionFilter.Actions.FirstOrDefault(x => x.EndsWith(message.Headers.Action));
                    if (String.IsNullOrEmpty(actionName))
                    {
                        actionName = actionFilter.Actions.FirstOrDefault(x => x.EndsWith(message.Headers.Action+"Request"));
                    }
                    if (!String.IsNullOrEmpty(actionName))
                    {
                        message.Headers.Action = actionName;
                        receiveLogger.Trace("调整Action：{0}---->{1}", oldAction, message.Headers.Action);
                    }
                }
            }

            bool isMatch = filter.Match(message);
            if (!isMatch)
            {

                receiveLogger.Error("收到消息，但是Action不匹配：\r\n地址：{0} \r\n消息：\r\n{1}\r\n", address.Uri.ToString(), message.ToString());

            }
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