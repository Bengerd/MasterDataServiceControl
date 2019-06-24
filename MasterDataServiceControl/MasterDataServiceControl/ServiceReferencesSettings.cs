using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using MasterDataServiceControl.MasterDataServices;

namespace MasterDataServiceControl
{
    public static class ServiceReferencesSettings
    {
        public static ServiceClient clientProxy = null;

        public static string TargetURL { get; set; } = string.Empty;
        public static string EndpointIdentityUPN { get; set; } = string.Empty;


        public static ServiceClient GetClientProxy()
        {
            EndpointIdentity endpointIdentity = null;

            if (!string.IsNullOrWhiteSpace(EndpointIdentityUPN))
            {
                endpointIdentity = EndpointIdentity.CreateUpnIdentity(EndpointIdentityUPN);
            }

            EndpointAddress endptAddress;
            // Create an endpoint address using the URL. 
            if (!string.IsNullOrWhiteSpace(TargetURL))
            {
                if (!string.IsNullOrWhiteSpace(EndpointIdentityUPN))
                {
                    Uri uri = new Uri(TargetURL);
                    endptAddress = new EndpointAddress(uri, endpointIdentity);
                }
                else
                {
                    endptAddress = new EndpointAddress(TargetURL);
                }
                
            }
            else
            {
                throw new Exception("Undefine Master data services URL");
            }

            // Create and configure the WS Http binding. 
            WSHttpBinding wsBinding = new WSHttpBinding();
            wsBinding.MaxReceivedMessageSize = Int32.MaxValue;

            // Create and return the client proxy. 
            return new ServiceClient(wsBinding, endptAddress);
        }

        public static void HandleOperationErrors(OperationResult result)
        {
            string errorMessage = string.Empty;

            if (result.Errors.Count() > 0)
            {
                foreach (Error anError in result.Errors)
                {
                    errorMessage += "Operation Error: " + anError.Code + ":" + anError.Description + "\n";
                }
                // Show the error messages.
                Console.WriteLine(errorMessage);
                // Throw an exception.
                throw new Exception(errorMessage);
            }
        }
    }
}
