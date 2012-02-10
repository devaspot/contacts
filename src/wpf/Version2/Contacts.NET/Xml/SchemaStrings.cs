
namespace Microsoft.Communications.Contacts.Xml
{
    using System.Collections.Generic;
    using Standard;
    using System;

    internal static class SchemaStrings
    {
        private static readonly KeyValuePair<string, string>[] _CollectionNameToNodeMap = new[]
        {
            new KeyValuePair<string, string>(PropertyNames.NameCollection,            PropertyNames.NameArrayNodeRaw),
            new KeyValuePair<string, string>(PropertyNames.EmailAddressCollection,    PropertyNames.EmailAddressArrayNodeRaw),
            new KeyValuePair<string, string>(PropertyNames.PhoneNumberCollection,     PropertyNames.PhoneNumberArrayNodeRaw),
            new KeyValuePair<string, string>(PropertyNames.PhysicalAddressCollection, PropertyNames.PhysicalAddressArrayNodeRaw),
            new KeyValuePair<string, string>(PropertyNames.PersonCollection,          PropertyNames.PersonArrayNodeRaw),
            new KeyValuePair<string, string>(PropertyNames.DateCollection,            PropertyNames.DateArrayNodeRaw),
            new KeyValuePair<string, string>(PropertyNames.UrlCollection,             PropertyNames.UrlArrayNodeRaw),
            new KeyValuePair<string, string>(PropertyNames.IMAddressCollection,       PropertyNames.IMAddressArrayNodeRaw),
            new KeyValuePair<string, string>(PropertyNames.PhotoCollection,           PropertyNames.PhotoArrayNodeRaw),
            new KeyValuePair<string, string>(PropertyNames.CertificateCollection,     PropertyNames.CertificateArrayNodeRaw),
            new KeyValuePair<string, string>(PropertyNames.PositionCollection,        PropertyNames.PositionArrayNodeRaw),
            new KeyValuePair<string, string>(PropertyNames.ContactIdCollection,       PropertyNames.ContactIdArrayNodeRaw),
        };

        private static readonly string[] ElementSkipList = new[]
        {
            ContactRootElement,
            ExtensionsRootElement,
            Label,
            LabelCollection,
        };

        /// <summary>"binary"</summary>
        public const string DefaultMimeType = "binary";

        /// <summary>"http://schemas.microsoft.com/Contact"</summary>
        public const string ContactNamespace = "http://schemas.microsoft.com/Contact";

        public const string ContactRootElement = "contact";
        
        /// <summary>"http://www.w3.org/2001/XMLSchema-instance"</summary>
        public const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        
        /// <summary>"http://schemas.microsoft.com/Contact/Extended"</summary>
        public const string ExtensionsNamespace = "http://schemas.microsoft.com/Contact/Extended";

        /// <summary>"Extended"</summary>
        public const string ExtensionsRootElement = "Extended";

        /// <summary>Version</summary>
        public const string Version = "Version";

        /// <summary>ModificationDate</summary>
        public const string ModificationDate = "ModificationDate";

        /// <summary>"type", an attribute used by simple extensions to indicate the XmlNode's type.</summary>
        public const string NodeType = "type";

        /// <summary>ElementID</summary>
        public const string ElementId = "ElementID";

        /// <summary>"ContentType", an attribute used to decorate Binary property nodes to indicate their MIME type.</summary>
        public const string ContentType = "ContentType";

        /// <summary>"nil"</summary>
        public const string Nil = "nil";

        /// <summary>"arrayElement"</summary>
        public const string SchemaTypeArrayElement = "arrayElement";

        /// <summary>"arrayNode"</summary>
        public const string SchemaTypeArrayNode = "arrayNode";

        /// <summary>"string"</summary>
        public const string SchemaTypeString = "string";

        /// <summary>"dateTime"</summary>
        public const string SchemaTypeDate = "dateTime";

        /// <summary>"binary"</summary>
        public const string SchemaTypeBinary = "binary";

        /// <summary>"Label"</summary>
        public const string Label = "Label";

        /// <summary>"LabelCollection"</summary>
        public const string LabelCollection = "LabelCollection";

        /// <summary>"true"</summary>
        public const string True = "true";

        public static string TryGetCollectionNodeName(string collectionName)
        {
            Assert.IsNeitherNullNorEmpty(collectionName);
            // Since KeyValuePair<,> is a struct if this fails to find an appropriate value then
            // a non-null default will be returned, so it's okay to return Value (which will be null).
            return Array.Find(_CollectionNameToNodeMap, pair => pair.Key == collectionName).Value;
        }

        public static bool SkipPropertyName(string propertyName)
        {
            return -1 != Array.IndexOf(ElementSkipList, propertyName);
        }
    }
}
