using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using MasterDataServiceControl.MasterDataServices;

namespace MasterDataServiceControl
{
    public static class MetaData
    {
        public static Collection<MasterDataServices.Attribute> GetEntityAttributes(string model, string entity)
        {
            Collection<MasterDataServices.MetadataAttribute> metadataAttributes;
            var attributes = new Collection<MasterDataServices.Attribute>();

            try
            {
                // Create the request object for getting attribute information.
                var getRequest = new MetadataGetRequest();
                getRequest.SearchCriteria = new MetadataSearchCriteria();
                getRequest.SearchCriteria.SearchOption = SearchOption.UserDefinedObjectsOnly;
                // Set model id, entity id, and attribute name.
                getRequest.SearchCriteria.Entities = new Collection<Identifier> { new Identifier { Name = entity } };
                getRequest.SearchCriteria.Models = new Collection<Identifier> { new Identifier { Name = model } };
                getRequest.ResultOptions = new MetadataResultOptions();
                getRequest.ResultOptions.Attributes = ResultType.Details;

                // Get an attribute information.
                MetadataGetResponse getResponse = ServiceReferencesSettings.clientProxy.MetadataGet(getRequest);

                metadataAttributes = getResponse.Metadata.Attributes;

                ServiceReferencesSettings.HandleOperationErrors(getResponse.OperationResult);

                foreach (var item in metadataAttributes)
                {
                    var attribute = new MasterDataServices.Attribute();

                    if (item.AttributeType == AttributeType.Domain || item.AttributeType == AttributeType.File)
                    {
                        switch (item.AttributeType)
                        {
                            case AttributeType.Domain:
                                attribute.Type = AttributeValueType.Domain;
                                break;
                            case AttributeType.File:
                                attribute.Type = AttributeValueType.File;
                                break;
                        }
                    }
                    else if (item.AttributeType == AttributeType.FreeForm)
                    {
                        switch (item.DataType)
                        {
                            case AttributeDataType.DateTime:
                                attribute.Type = AttributeValueType.DateTime;
                                break;
                            case AttributeDataType.Number:
                                attribute.Type = AttributeValueType.Number;
                                break;
                            case AttributeDataType.Text:
                                attribute.Type = AttributeValueType.String;
                                break;
                        }
                    }
                    attributes.Add(attribute);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex);
            }

            //return AttributeIdentifier;

            return attributes;
        }
    }
}
