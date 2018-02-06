#if WCF_SUPPORTED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Dynamic.Net.WCF
{
    public class AdjustActionNameServiceBehavior : IServiceBehavior
    {
#region IServiceBehavior 成员

        public void AddBindingParameters(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            return;
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            
            foreach (ChannelDispatcher chDisp in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher epDisp in chDisp.Endpoints)
                {
                    epDisp.AddressFilter = new AddressConverterFilter(epDisp.AddressFilter, serviceHostBase.Description.ServiceType, epDisp.EndpointAddress);
                    epDisp.ContractFilter = new ActionConverterFilter(epDisp.ContractFilter, serviceHostBase.Description.ServiceType, epDisp.EndpointAddress);
                }
              
            }
        }

      

        public void Validate(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            return;
        }

#endregion
    }
}
#endif