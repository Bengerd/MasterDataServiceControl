using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using MasterDataServiceControl.MasterDataServices;


namespace MasterDataServiceControl
{
    public static class MasterData
    {

        public static void CreateEntityMember(string modelName, string versionName, string entityName, MemberType memberType
            , string aNewMemberName, string aNewMemberCode = null, Collection<MasterDataServices.Attribute> attributes = null
            , string hierarchyName = null, string changesetName = null)
        {
            // Create the request object for entity creation.
            EntityMembersCreateRequest createRequest = new EntityMembersCreateRequest();
            createRequest.Members = new EntityMembers();
            createRequest.ReturnCreatedIdentifiers = true;
            // Set the modelId, versionId, and entityId.
            createRequest.Members.ModelId = new Identifier { Name = modelName };
            createRequest.Members.VersionId = new Identifier { Name = versionName };
            createRequest.Members.EntityId = new Identifier { Name = entityName };
            createRequest.Members.MemberType = memberType;
            createRequest.Members.Members = new Collection<Member> { };
            Member aNewMember = new Member();

            if (!string.IsNullOrEmpty(aNewMemberCode))
            {
                aNewMember.MemberId = new MemberIdentifier() { MemberType = memberType, Code = aNewMemberCode };
            }
            else if (!string.IsNullOrEmpty(aNewMemberName))
            {
                aNewMember.MemberId = new MemberIdentifier() { MemberType = memberType, Name = aNewMemberName };
            }
            else
            {
                aNewMember.MemberId = new MemberIdentifier() { MemberType = memberType };
            };


            if (attributes != null)
            {
                aNewMember.Attributes = attributes;
            }

            if (memberType == MemberType.Consolidated)
            {
                // In case when the member type is consolidated set the parent information.
                // Set the hierarchy name and the parent code ("ROOT" means the root node of the hierarchy).
                aNewMember.Parents = new Collection<Parent> { };
                Parent aParent = new Parent();
                aParent.HierarchyId = new Identifier() { Name = hierarchyName };
                aParent.ParentId = new MemberIdentifier() { Code = "ROOT" };
                aNewMember.Parents.Add(aParent);
            }

            if (!string.IsNullOrEmpty(changesetName))
            {
                createRequest.Members.ChangesetId = new Identifier { Name = changesetName };
            }

            createRequest.Members.Members.Add(aNewMember);

            // Create a new entity member
            EntityMembersCreateResponse createResponse = ServiceReferencesSettings.clientProxy.EntityMembersCreate(createRequest);
            ServiceReferencesSettings.HandleOperationErrors(createResponse.OperationResult);

        }

        // Get the entity member identifier with specified model name, version name, entity name, member type, and entity member name.
        public static Collection<Member> GetEntityMembers(string modelName, string versionName, string entityName, MemberType memberType, string enchancedFilter = null)
        {
            MemberIdentifier memberIdentifier = new MemberIdentifier();

            // Create the request object to get the entity information.
            EntityMembersGetRequest getRequest = new EntityMembersGetRequest();
            getRequest.MembersGetCriteria = new EntityMembersGetCriteria();

            // Set the modelId, versionId, entityId, and the member name.
            getRequest.MembersGetCriteria.ModelId = new Identifier { Name = modelName };
            getRequest.MembersGetCriteria.VersionId = new Identifier { Name = versionName };
            getRequest.MembersGetCriteria.EntityId = new Identifier { Name = entityName };
            getRequest.MembersGetCriteria.MemberType = memberType;
            getRequest.MembersGetCriteria.MemberReturnOption = MemberReturnOption.Data;

            if (!string.IsNullOrWhiteSpace(enchancedFilter))
            {
                if (!string.IsNullOrWhiteSpace(getRequest.MembersGetCriteria.SearchTerm))
                    getRequest.MembersGetCriteria.SearchTerm = $"{getRequest.MembersGetCriteria.SearchTerm} AND {enchancedFilter}";
                else
                    getRequest.MembersGetCriteria.SearchTerm = $"{enchancedFilter}";
            }

            // Get the entity member information
            EntityMembersGetResponse getResponse = ServiceReferencesSettings.clientProxy.EntityMembersGet(getRequest);

            ServiceReferencesSettings.HandleOperationErrors(getResponse.OperationResult);

            return getResponse.EntityMembers.Members;
        }

        public static Member GetEntityMember(string modelName, string versionName, string entityName, MemberType memberType, string memberCode = null, string memberName = null, string enchancedFilter = null)
        {
            MemberIdentifier memberIdentifier = new MemberIdentifier();

            // Create the request object to get the entity information.
            EntityMembersGetRequest getRequest = new EntityMembersGetRequest();
            getRequest.MembersGetCriteria = new EntityMembersGetCriteria();

            // Set the modelId, versionId, entityId, and the member name.
            getRequest.MembersGetCriteria.ModelId = new Identifier { Name = modelName };
            getRequest.MembersGetCriteria.VersionId = new Identifier { Name = versionName };
            getRequest.MembersGetCriteria.EntityId = new Identifier { Name = entityName };
            getRequest.MembersGetCriteria.MemberType = memberType;
            getRequest.MembersGetCriteria.MemberReturnOption = MemberReturnOption.Data;

            if (!string.IsNullOrWhiteSpace(memberCode))
                getRequest.MembersGetCriteria.SearchTerm = $"Code = '{memberCode}'";

            if (!string.IsNullOrWhiteSpace(memberName))
            {
                if (!string.IsNullOrWhiteSpace(getRequest.MembersGetCriteria.SearchTerm))
                    getRequest.MembersGetCriteria.SearchTerm = $"{getRequest.MembersGetCriteria.SearchTerm} AND Name = '{memberName}'";
                else
                    getRequest.MembersGetCriteria.SearchTerm = $"Name = '{memberName}'";
            }

            if (!string.IsNullOrWhiteSpace(enchancedFilter))
            {
                if (!string.IsNullOrWhiteSpace(getRequest.MembersGetCriteria.SearchTerm))
                    getRequest.MembersGetCriteria.SearchTerm = $"{getRequest.MembersGetCriteria.SearchTerm} AND {enchancedFilter}";
                else
                    getRequest.MembersGetCriteria.SearchTerm = $"{enchancedFilter}";
            }

            // Get the entity member information
            EntityMembersGetResponse getResponse = ServiceReferencesSettings.clientProxy.EntityMembersGet(getRequest);

            Member member = null;

            if (getResponse.EntityMembers.Members.Count > 0)
                member = getResponse.EntityMembers.Members[0];

            ServiceReferencesSettings.HandleOperationErrors(getResponse.OperationResult);

            return member;
        }

        // Update an entity member (change name) with the member code.
        public static void UpdateEntityMember(string modelName, string versionName, string entityName, string memberCode, MemberType memberType
            , string updatedField, string updatedFieldValue, string changesetName = null)
        {
            // Create the request object for entity update.
            EntityMembersUpdateRequest updateRequest = new EntityMembersUpdateRequest();
            updateRequest.Members = new EntityMembers();
            // Set the modelName, the versionName, and the entityName.
            updateRequest.Members.ModelId = new Identifier { Name = modelName };
            updateRequest.Members.VersionId = new Identifier { Name = versionName };
            updateRequest.Members.EntityId = new Identifier { Name = entityName };
            updateRequest.Members.MemberType = MemberType.Leaf;
            updateRequest.Members.Members = new Collection<Member> { };
            Member aMember = new Member();
            // Set the member code.
            aMember.MemberId = new MemberIdentifier() { Code = memberCode, MemberType = memberType };
            aMember.Attributes = new Collection<MasterDataServices.Attribute> { };
            // Set the new member name into the Attribute object. 
            MasterDataServices.Attribute anAttribute = new MasterDataServices.Attribute();

            anAttribute.Identifier = new Identifier() { Name = updatedField };
            anAttribute.Type = AttributeValueType.String;
            anAttribute.Value = updatedFieldValue;
            aMember.Attributes.Add(anAttribute);
            updateRequest.Members.Members.Add(aMember);

            if (!string.IsNullOrEmpty(changesetName))
            {
                updateRequest.Members.ChangesetId = new Identifier { Name = changesetName };
            }

            // Update the entity member (change the name).
            EntityMembersUpdateResponse createResponse = ServiceReferencesSettings.clientProxy.EntityMembersUpdate(updateRequest);

            ServiceReferencesSettings.HandleOperationErrors(createResponse.OperationResult);

        }
    }
}
