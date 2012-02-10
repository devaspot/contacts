
namespace Microsoft.Communications.Contacts.Xml
{
    using System.Xml;
    using System.Collections.Generic;
using Standard;
    using System;
    using System.Globalization;

    internal class XmlElementManager
    {
        private readonly XmlDocument _document;
        private readonly string _contactNamespacePrefix;
        private readonly string _xsiNamespacePrefix;

        private readonly Dictionary<string, string> _simpleExtensionMap;

        private void _EnsureSimpleExtensionNamespace(string namespacePrefix)
        {
            string simpleUri;
            if (_simpleExtensionMap.TryGetValue(namespacePrefix, out simpleUri))
            {
                // Ensure that this is a simple extension uri.
                // We're simply not going to support callers who try to do things
                // like use "xsi" as a simple extension namespace.
                if (0 != string.Compare(simpleUri, SchemaStrings.ExtensionsNamespace + "/" + namespacePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    throw new SchemaException("Attempting to use a predeclared XML namespace as a simple extension.");
                }
                return;
            }

            XmlAttribute attrib = _document.CreateAttribute("xmlns:" + namespacePrefix);
            attrib.Value = SchemaStrings.ExtensionsNamespace + "/" + namespacePrefix;
            _document.DocumentElement.Attributes.Append(attrib);
            _simpleExtensionMap.Add(namespacePrefix, attrib.Value);
        }

        public XmlElementManager(XmlDocument document)
        {
            Assert.IsNotNull(document);
            _document = document;

            _simpleExtensionMap = new Dictionary<string, string>();

            // find all the xmlns declarations.
            foreach (XmlAttribute attrib in _document.DocumentElement.Attributes)
            {
                string name = attrib.Name;
                string prefix = null;

                if (name == "xmlns")
                {
                    // Default namespace.
                    prefix = string.Empty;
                }
                else if (name.StartsWith("xmlns:", StringComparison.Ordinal))
                {
                    Assert.AreNotEqual("xmlns:", name);
                    prefix = name.Substring("xmlns:".Length);
                }
                else
                {
                    // Not a namespace declaration
                    continue;
                }

                string uri = attrib.Value;

                if (uri.Equals(SchemaStrings.ContactNamespace, StringComparison.OrdinalIgnoreCase))
                {
                    // XML parsing should have choked if there were multiple declarations of this, yea?
                    Assert.IsNull(_contactNamespacePrefix);

                    _contactNamespacePrefix = prefix;
                }
                else if (uri.Equals(SchemaStrings.XsiNamespace, StringComparison.OrdinalIgnoreCase))
                {
                    Assert.IsNull(_xsiNamespacePrefix);

                    _xsiNamespacePrefix = prefix;
                }
                else if (uri.StartsWith(SchemaStrings.ExtensionsNamespace, StringComparison.OrdinalIgnoreCase))
                {
                    // Only going to support simple extensions where the prefix matches the URI (including case).
                    if (!("/" + prefix).Equals(uri.Substring(SchemaStrings.ExtensionsNamespace.Length), StringComparison.Ordinal))
                    {
                        throw new SchemaException("Improperly formed simple extension namespace declaration on root contact element.");
                    }

                    // This will throw if there are duplicate declarations of the same extension namespace prefix.
                    _simpleExtensionMap.Add(prefix, uri);
                }
            }
        }

        public XmlElement CreateSchemaElement(string arrayNodeName)
        {
            return _document.CreateElement(_contactNamespacePrefix, arrayNodeName, SchemaStrings.ContactNamespace);
        }

        public XmlAttribute CreateElementIdAttribute()
        {
            return _document.CreateAttribute(_contactNamespacePrefix, SchemaStrings.ElementId, SchemaStrings.ContactNamespace);
        }

        public void AddNilAttribute(XmlNode xmlNode)
        {
            Assert.IsNotNull(xmlNode);

            var nilAttribute = xmlNode.Attributes.GetNamedItem(SchemaStrings.Nil, SchemaStrings.XsiNamespace) as XmlAttribute;
            if (null == nilAttribute)
            {
                nilAttribute = xmlNode.OwnerDocument.CreateAttribute(_xsiNamespacePrefix, SchemaStrings.Nil, SchemaStrings.XsiNamespace);
                xmlNode.Attributes.Append(nilAttribute);
            }
            nilAttribute.Value = SchemaStrings.True;
        }

        public void UpdateVersionAndModificationDate(XmlNode arrayXNode)
        {
            Assert.IsNotNull(arrayXNode);

            var versionAttribute = arrayXNode.Attributes.GetNamedItem(SchemaStrings.Version, SchemaStrings.ContactNamespace) as XmlAttribute;
            int versionNumber = 0;
            if (null == versionAttribute)
            {
                versionAttribute = arrayXNode.OwnerDocument.CreateAttribute(_contactNamespacePrefix, SchemaStrings.Version, SchemaStrings.ContactNamespace);
                arrayXNode.Attributes.Append(versionAttribute);
            }
            else
            {
                versionNumber = int.Parse(versionAttribute.Value, CultureInfo.InvariantCulture);
            }
            ++versionNumber;
            versionAttribute.Value = versionNumber.ToString(CultureInfo.InvariantCulture);

            var modificationAttribute = arrayXNode.Attributes.GetNamedItem(SchemaStrings.ModificationDate, SchemaStrings.ContactNamespace) as XmlAttribute;
            if (null == modificationAttribute)
            {
                modificationAttribute = arrayXNode.OwnerDocument.CreateAttribute(_contactNamespacePrefix, SchemaStrings.ModificationDate, SchemaStrings.ContactNamespace);
                arrayXNode.Attributes.Append(modificationAttribute);
            }
            modificationAttribute.Value = XmlUtil.DateTimeNowString;

        }

        public XmlAttribute CreateNodeTypeAttribute(string nodeType)
        {
            XmlAttribute attrib = _document.CreateAttribute(_contactNamespacePrefix, SchemaStrings.NodeType, SchemaStrings.ContactNamespace);
            attrib.Value = nodeType;
            return attrib;
        }

        public XmlElement CreateExtensionElement(string extensionNamespace, string elementName)
        {
            // CONSIDER: Normalizing the prefix.
            _EnsureSimpleExtensionNamespace(extensionNamespace);
  
            return _document.CreateElement(extensionNamespace, elementName, _simpleExtensionMap[extensionNamespace]);
        }

        public void SetMimeTypeAttribute(XmlNode childNode, string mimeType)
        {
            Assert.IsNotNull(childNode);
            Assert.IsNeitherNullNorEmpty(mimeType);

            var mimeAttribute = childNode.Attributes.GetNamedItem(SchemaStrings.ContentType, SchemaStrings.ContactNamespace) as XmlAttribute;
            if (null == mimeAttribute)
            {
                mimeAttribute = childNode.OwnerDocument.CreateAttribute(_contactNamespacePrefix, SchemaStrings.ContentType, SchemaStrings.ContactNamespace);
                childNode.Attributes.Append(mimeAttribute);
            }
            mimeAttribute.Value = mimeType;
        }

        public void SetSchemaTypeAttribute(XmlNode childNode, string schemaType)
        {
            Assert.IsNotNull(childNode);
            Assert.IsTrue(PropertyNode.IsExtendedNamespace(childNode.NamespaceURI));
            Assert.IsNeitherNullNorEmpty(schemaType);

            var typeAttribute = childNode.Attributes.GetNamedItem(SchemaStrings.NodeType, SchemaStrings.ContactNamespace) as XmlAttribute;
            if (null == typeAttribute)
            {
                typeAttribute = childNode.OwnerDocument.CreateAttribute(_contactNamespacePrefix, SchemaStrings.NodeType, SchemaStrings.ContactNamespace);
                childNode.Attributes.Append(typeAttribute);
            }
            typeAttribute.Value = schemaType;
        }
    }
}
