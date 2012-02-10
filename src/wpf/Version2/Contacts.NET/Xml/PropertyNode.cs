/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;
    using Standard;
    
    [Flags]
    internal enum NodeTypes
    {
        /// <summary>Sentinal empty value.</summary>
        None = 0,
        /// <summary>"string", "dateTime", "binary"</summary>
        Item = 1,
        /// <summary>"emailAddressCollection"</summary>
        ElementCollection = 2,
        /// <summary>"emailAddress"</summary>
        ElementNode = 4,
        /// <summary>"contact" or "Extended"</summary>
        RootElement = 8,

        Any = Item | ElementCollection | ElementNode | RootElement,
    }

    internal class PropertyNode : IEnumerable<PropertyNode>, IEquatable<PropertyNode>
    {
        // Shared empty list used for nodes with no labels.
        private static readonly IList<string> _EmptyLabelList = new List<string>().AsReadOnly();

        private enum _NamespaceType
        {
            Invalid,
            Contact,
            Extended,
        }

        #region Fields
        private readonly string _localName;
        private DateTime? _modificationDate;
        private readonly PropertyNode _parent;
        private List<PropertyNode> _childrenList;
        private List<string> _labels;
        private readonly string _contentType;
        private readonly _NamespaceType _nsType;
        private string _icontactName;
        private readonly string _simpleExtensionNamespace;
        private readonly string _simpleExtensionPrefix;
        private string _typeAttribute;
        #endregion

        #region Auto Properties
        public bool XsiNil { get; set; }
        public Guid? ElementId { get; private set; }
        public XmlNode XmlNode { get; private set; }
        public int Version { get; set; }
        public string Value { get; set; }
        #endregion

        private static _NamespaceType _GetNamespaceType(string uri)
        {
            const string extensionsNamespacePlusSlash = SchemaStrings.ExtensionsNamespace + "/";
            Assert.IsFalse(string.IsNullOrEmpty(uri));

            // CONSIDER: Should these not be case sensitive checks?

            if (uri.Equals(SchemaStrings.ContactNamespace, StringComparison.Ordinal))
            {
                return _NamespaceType.Contact;
            }

            // Not sure whether slashes should be supported in simple extension namespaces.
            if (uri.StartsWith(extensionsNamespacePlusSlash, StringComparison.Ordinal)
                && uri.Length > extensionsNamespacePlusSlash.Length
                && uri.LastIndexOf('/') == extensionsNamespacePlusSlash.Length - 1)
            {
                return _NamespaceType.Extended;
            }

            return _NamespaceType.Invalid;
        }

        public static bool IsValidContactNamespace(string uri)
        {
            return _GetNamespaceType(uri) != _NamespaceType.Invalid;
        }

        public static bool IsExtendedNamespace(string uri)
        {
            return _GetNamespaceType(uri) == _NamespaceType.Extended;
        }

        public PropertyNode(XmlReader currentNode, PropertyNode parent)
        {
            // Using only the reader, so we don't keep an XmlNode reference since this won't be modified.
            XmlNode = null;

            // Callers should enforce this.
            Assert.IsTrue(IsValidContactNamespace(currentNode.NamespaceURI));

            _nsType = _GetNamespaceType(currentNode.NamespaceURI);
            if (_nsType == _NamespaceType.Extended)
            {
                _simpleExtensionNamespace = currentNode.NamespaceURI;
                _simpleExtensionPrefix = "[" + currentNode.NamespaceURI.Substring(SchemaStrings.ExtensionsNamespace.Length + 1) + "]";
            }
            _localName = currentNode.LocalName;

            if (currentNode.HasAttributes)
            {
                while (currentNode.MoveToNextAttribute())
                {
                    switch (currentNode.NamespaceURI)
                    {
                        case SchemaStrings.ContactNamespace:
                            // Can safely convert strings to their CLR type here - the XSD should enforce the content.
                            switch (currentNode.LocalName)
                            {
                                case SchemaStrings.Version:
                                    Version = int.Parse(currentNode.Value, NumberStyles.Integer, null);
                                    break;
                                case SchemaStrings.ModificationDate:
                                    _modificationDate = DateTime.Parse(currentNode.Value, null, DateTimeStyles.AdjustToUniversal);
                                    break;
                                case SchemaStrings.NodeType:
                                    _typeAttribute = currentNode.Value;
                                    break;
                                case SchemaStrings.ElementId:
                                    ElementId = new Guid(currentNode.Value);
                                    break;
                                case SchemaStrings.ContentType:
                                    _contentType = currentNode.Value;
                                    break;
                            }
                            break;
                        case SchemaStrings.XsiNamespace:
                            switch (currentNode.LocalName)
                            {
                                case SchemaStrings.Nil:
                                    // CONSIDER: Case sensitive comparison?
                                    XsiNil = currentNode.Value == "1" || currentNode.Value == "true";
                                    break;
                            }
                            break;
                    }
                }
                // Move the reader back out of the attribute list.
                currentNode.MoveToElement();
            }

            Assert.Implies(null == parent, LocalName == SchemaStrings.ContactRootElement);
            if (null != parent)
            {
                _parent = parent;
                parent.Children.Add(this);
            }
        }

        public PropertyNode(XmlNode currentNode, PropertyNode parent, bool append, string value, string propertyName)
            : this(currentNode, parent, append)
        {
            Assert.IsNeitherNullNorEmpty(value);
            Assert.IsNeitherNullNorEmpty(propertyName);

            Value = value;
            IContactName = propertyName;
        }
        /// <summary>
        /// Creates a new PropertyNode with partial data.
        /// The properties that still need to be provided are:
        /// * Value - The text value of the node.
        /// * Labels - Can be populated with ProcessLabels given the appropriate XmlNode.
        /// * IContactName - the path for IContactProperties.
        /// </summary>
        /// <param name="currentNode">The XmlNode that this represents.</param>
        /// <param name="parent">The parent PropertyNode of this new node.</param>
        public PropertyNode(XmlNode currentNode, PropertyNode parent)
            : this(currentNode, parent, true)
        { 
        }

        /// <summary>
        /// Creates a new PropertyNode with partial data.
        /// The properties that still need to be provided are:
        /// * Value - The text value of the node.
        /// * Labels - Can be populated with ProcessLabels given the appropriate XmlNode.
        /// * IContactName - the path for IContactProperties.
        /// </summary>
        /// <param name="currentNode">The XmlNode that this represents.</param>
        /// <param name="parent">The parent PropertyNode of this new node.</param>
        /// <param name="append">If this is a child, whether to append or prepend it to the parent's Children collection.</param>
        public PropertyNode(XmlNode currentNode, PropertyNode parent, bool append)
        {
            XmlNode = currentNode;

            // Callers should enforce this.
            Assert.IsTrue(IsValidContactNamespace(currentNode.NamespaceURI));

            _nsType = _GetNamespaceType(currentNode.NamespaceURI);
            if (_nsType == _NamespaceType.Extended)
            {
                _simpleExtensionNamespace = currentNode.NamespaceURI;
                _simpleExtensionPrefix = "[" + currentNode.NamespaceURI.Substring(SchemaStrings.ExtensionsNamespace.Length + 1) + "]";
            }
            _localName = currentNode.LocalName;

            foreach (XmlAttribute attribute in currentNode.Attributes)
            {
                switch (attribute.NamespaceURI)
                {
                    case SchemaStrings.ContactNamespace:
                        // Can safely convert strings to their CLR type here - the XSD should enforce the content.
                        switch (attribute.LocalName)
                        {
                            case SchemaStrings.Version:
                                Version = int.Parse(attribute.Value, NumberStyles.Integer, null);
                                break;
                            case SchemaStrings.ModificationDate:
                                _modificationDate = DateTime.Parse(attribute.Value, null, DateTimeStyles.AdjustToUniversal);
                                break;
                            case SchemaStrings.NodeType:
                                _typeAttribute = attribute.Value;
                                break;
                            case SchemaStrings.ElementId:
                                ElementId = new Guid(attribute.Value);
                                break;
                            case SchemaStrings.ContentType:
                                _contentType = attribute.Value;
                                break;
                        }
                        break;
                    case SchemaStrings.XsiNamespace:
                        switch (attribute.LocalName)
                        {
                            case SchemaStrings.Nil:
                                // CONSIDER: Case sensitive comparison?
                                XsiNil = attribute.Value == "1" || attribute.Value == SchemaStrings.True;
                                break;
                        }
                        break;

                }
            }

            Assert.Implies(null == parent, LocalName == SchemaStrings.ContactRootElement);
            if (null != parent)
            {
                _parent = parent;
                int insertIndex = append ? parent.Children.Count : 0;
                parent.Children.Insert(insertIndex, this);
            }
        }

        #region Public Properties

        public List<PropertyNode> Children
        {
            get
            {
                if (null == _childrenList)
                {
                    _childrenList = new List<PropertyNode>();
                }
                return _childrenList;
            }
        }

        public NodeTypes ContactNodeType
        {
            get
            {
                if (LocalName == SchemaStrings.ContactRootElement
                    || LocalName == SchemaStrings.ExtensionsRootElement)
                {
                    return NodeTypes.RootElement;
                }

                // Schematized properties should get this from the XSD.
                // It's not always set on this when the property is created, so update this if it's available.
                if (string.IsNullOrEmpty(_typeAttribute) && null != XmlNode)
                {
                    XmlNode typeNode = XmlNode.Attributes.GetNamedItem(SchemaStrings.NodeType, SchemaStrings.ContactNamespace);
                    if (null != typeNode)
                    {
                        _typeAttribute = typeNode.Value;
                    }
                }

                if (!string.IsNullOrEmpty(_typeAttribute))
                {
                    switch (_typeAttribute)
                    {
                        case SchemaStrings.SchemaTypeArrayElement:
                            return NodeTypes.ElementCollection;
                        case SchemaStrings.SchemaTypeArrayNode:
                            return NodeTypes.ElementNode;
                        default:
                            return NodeTypes.Item;
                    }
                }

                if (null != ElementId)
                {
                    return NodeTypes.ElementNode;
                }

                if (!string.IsNullOrEmpty(IContactName))
                {
                    var pni = new PropertyNameInfo(IContactName);
                    if (pni.Type == PropertyNameTypes.SchematizedCollectionName)
                    {
                        return NodeTypes.ElementCollection;
                    }
                    if (pni.Type == PropertyNameTypes.SchematizedNode)
                    {
                        return NodeTypes.ElementNode;
                    }
                }

                return NodeTypes.Item;
            }
        }

        public ContactPropertyType ContactPropertyType
        {
            get
            {
                if (!string.IsNullOrEmpty(_typeAttribute))
                {
                    switch (_typeAttribute)
                    {
                        case SchemaStrings.SchemaTypeArrayNode:
                            return ContactPropertyType.ArrayNode;
                        case SchemaStrings.SchemaTypeBinary:
                            return ContactPropertyType.Binary;
                        case SchemaStrings.SchemaTypeDate:
                            return ContactPropertyType.DateTime;
                        case SchemaStrings.SchemaTypeString:
                            return ContactPropertyType.String;
                        // Collections don't map to a ContactPropertyType.
                        case SchemaStrings.SchemaTypeArrayElement:
                            return ContactPropertyType.None;
                        default:
                            Assert.Fail();
                            return ContactPropertyType.None;
                    }
                }

                // If c:ContentType is set then this is a binary.
                if (!string.IsNullOrEmpty(_contentType))
                {
                    return ContactPropertyType.Binary;
                }

                NodeTypes nodeType = ContactNodeType;
                if (NodeTypes.ElementCollection == nodeType)
                {
                    return ContactPropertyType.None;
                }
                if (NodeTypes.ElementNode == nodeType)
                {
                    return ContactPropertyType.ArrayNode;
                }

                if (!string.IsNullOrEmpty(Value))
                {
                    DateTime canary;
                    if (DateTime.TryParse(Value, out canary))
                    {
                        return ContactPropertyType.DateTime;
                    }
                }

                return ContactPropertyType.String;
            }
        }

        /// <summary>
        /// Type of the Value when this represents a binary stream.  Only valid for binary values.
        /// </summary>
        /// <remarks>
        /// This property must not be empty if this is a binary value.
        /// </remarks>
        public string ContentType
        {
            get
            {
                Assert.Implies(ContactPropertyType.Binary == ContactPropertyType, !string.IsNullOrEmpty(_contentType));
                return _contentType ?? "";
            }
        }

        public string ExtendedNamespacePrefix
        {
            get { return _simpleExtensionPrefix ?? ""; }
        }

        public string ExtendedNamespace
        {
            get { return _simpleExtensionNamespace ?? ""; }
        }

        // CONSIDER: whether IContactName can be done instead in the constructor.
        public string IContactName
        {
            get { return _icontactName ?? ""; }
            set { _icontactName = value; }
        }

        public IList<string> Labels
        {
            get
            {
                if (null == _labels)
                {
                    return _EmptyLabelList;
                }
                return _labels.AsReadOnly();
            }
        }

        public string LocalName
        {
            get { return _localName; }
        }

        public DateTime ModificationDate
        {
            get
            {
                Assert.Implies(!_modificationDate.HasValue, null != Parent);
                return _modificationDate ?? Parent.ModificationDate;
            }
            private set { _modificationDate = value; }
        }

        public PropertyNode Parent
        {
            get { return _parent; }
        }

        #endregion

        /// <summary>
        /// Given that the reader currently sits on a LabelCollection node,
        /// progresses the reader past the element and adds the Label child elements to this node.
        /// </summary>
        /// <param name="reader"></param>
        public void ProcessLabels(XmlReader reader)
        {
            // This overload of ProcessLabels is different than the XmlNode version.
            // This should only be used for initial population.
            Assert.AreEqual(reader.LocalName, SchemaStrings.LabelCollection);

            if (reader.IsEmptyElement)
            {
                return;
            }

            Assert.IsNull(_labels);
            _labels = new List<string>();

            bool inLabelNode = false;
            bool skipped = false;
            while (skipped ? !reader.EOF : reader.Read())
            {
                skipped = false;
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.NamespaceURI == SchemaStrings.ContactNamespace)
                        {
                            // XSD should catch for other elements in the LabelCollection.
                            if (reader.LocalName == SchemaStrings.ExtensionsRootElement)
                            {
                                reader.Skip();
                                skipped = true;
                                continue; // while
                            }
                            if (reader.LocalName == SchemaStrings.Label)
                            {
                                Assert.IsFalse(inLabelNode);
                                inLabelNode = true;

                                if (reader.IsEmptyElement)
                                {
                                    goto case XmlNodeType.EndElement;
                                }
                            }
                            else
                            {
                                Assert.Fail();
                                throw new SchemaException("Invalid data in LabelCollection");
                            }
                        }

                        break;
                    case XmlNodeType.EndElement:
                        if (reader.NamespaceURI == SchemaStrings.ContactNamespace)
                        {
                            if (SchemaStrings.LabelCollection == reader.LocalName)
                            {
                                Assert.IsFalse(inLabelNode);
                                return;
                            }
                            Assert.AreEqual(SchemaStrings.Label, reader.LocalName);
                            Assert.IsTrue(inLabelNode);
                            inLabelNode = false;
                        }
                        break;
                    case XmlNodeType.Text:
                        if (inLabelNode)
                        {
                            if (!string.IsNullOrEmpty(reader.Value))
                            {
                                _labels.Add(reader.Value);
                            }
                        }
                        break;
                }
            }
            Assert.Fail();
        }

        public void ProcessLabels(XmlNode labelCollectionNode)
        {
            // This overload of ProcessLabels is different than the XmlReader version.
            // This can be used for initial population as well as reprocessing.
             _labels = null;

            if (null == labelCollectionNode)
            {
                return;
            }

            Assert.AreEqual(labelCollectionNode.LocalName, SchemaStrings.LabelCollection);

            if (!labelCollectionNode.HasChildNodes)
            {
                return;
            }

            _labels = new List<string>();

            foreach (XmlNode childNode in labelCollectionNode.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element)
                {
                    if (childNode.NamespaceURI == SchemaStrings.ContactNamespace)
                    {
                        // XSD should catch for other elements in the LabelCollection.
                        if (childNode.LocalName != SchemaStrings.ExtensionsRootElement)
                        {
                            if (childNode.LocalName == SchemaStrings.Label)
                            {
                                if (childNode.HasChildNodes)
                                {
                                    Assert.AreEqual(childNode.ChildNodes[0].NodeType, XmlNodeType.Text);
                                    _labels.Add(childNode.ChildNodes[0].Value);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void EnsureVersionAndModificationDate(DateTime defaultModificationDate)
        {
            Assert.AreNotEqual(default(DateTime), defaultModificationDate);
            Assert.AreEqual(DateTimeKind.Utc, defaultModificationDate.Kind);

            if (null == _modificationDate)
            {
                _modificationDate = defaultModificationDate;
            }

            if (Version < 1)
            {
                Version = 1;
            }
        }

        public bool MatchesLabels(string[] labelFilter, bool anyLabelMatches)
        {
            if (null == labelFilter || 0 == labelFilter.Length)
            {
                return true;
            }

            foreach (string label in labelFilter)
            {
                if (Labels.Contains(label))
                {
                    if (anyLabelMatches)
                    {
                        return true;
                    }
                }
                else
                {
                    if (!anyLabelMatches)
                    {
                        return false;
                    }
                }
            }

            return !anyLabelMatches;
        }

        #region IEnumerable<PropertyNode> Members

        /// <summary>Depth-first traveral of the PropertyNode tree.</summary>
        public IEnumerator<PropertyNode> GetEnumerator()
        {
            if (ContactNodeType != NodeTypes.RootElement)
            {
                yield return this;
            }

            if (null != _childrenList)
            {
                foreach (PropertyNode child in _childrenList)
                {
                    foreach (PropertyNode node in child)
                    {
                        yield return node;
                    }
                }
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Object Overrides

        public override bool Equals(object obj)
        {
            var other = obj as PropertyNode;
            if (null == other)
            {
                return false;
            }
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return IContactName.GetHashCode();
        }

        public override string ToString()
        {
            return IContactName + ":" + Value;
        }

        #endregion

        #region IEquatable<PropertyNode> Members

        public bool Equals(PropertyNode other)
        {
            if (ContactNodeType != other.ContactNodeType
                || ContactPropertyType != other.ContactPropertyType
                || ContentType != other.ContentType
                || ElementId != other.ElementId
                || ExtendedNamespace != other.ExtendedNamespace
                || ExtendedNamespacePrefix != other.ExtendedNamespacePrefix
                || IContactName != other.IContactName
                || LocalName != other.LocalName
                || Value != other.Value
                || Version != other.Version
                || XsiNil != other.XsiNil
                || Labels.Count != other.Labels.Count
                || Children.Count != other.Children.Count)
            {
                return false;
            }

            foreach (string label in Labels)
            {
                if (!other.Labels.Contains(label))
                {
                    return false;
                }
            }

            for(int i = 0; i < Children.Count; ++i)
            {
                if (!Children[i].Equals(other.Children[i]))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        [System.Diagnostics.Conditional("DEBUG")]
        internal void AssertEquals(PropertyNode other)
        {
            Assert.AreEqual(ContactNodeType, other.ContactNodeType);
            Assert.AreEqual(ContactPropertyType, other.ContactPropertyType);
            Assert.AreEqual(ContentType, other.ContentType);
            Assert.AreEqual(ElementId, other.ElementId);
            Assert.AreEqual(ExtendedNamespace, other.ExtendedNamespace);
            Assert.AreEqual(ExtendedNamespacePrefix, other.ExtendedNamespacePrefix);
            Assert.AreEqual(IContactName, other.IContactName);
            Assert.AreEqual(LocalName, other.LocalName);
            Assert.AreEqual(Value, other.Value);
            Assert.AreEqual(Version, other.Version);
            Assert.AreEqual(XsiNil, other.XsiNil);
            Assert.AreEqual(Labels.Count, other.Labels.Count);
            Assert.AreEqual(Children.Count, other.Children.Count);

            foreach (string label in Labels)
            {
                Assert.IsTrue(other.Labels.Contains(label));
            }

            for (int i = 0; i < Children.Count; ++i)
            {
                Children[i].AssertEquals(other.Children[i]);
            }
        }

        public PropertyNode CloneForSwap(bool copyChildren, bool copyLabels, XmlNode copyNode)
        {
            Assert.IsNotNull(copyNode);
            
            // Currently unsupported
            Assert.IsFalse(copyChildren);
            Assert.IsFalse(copyLabels);

            var copy = (PropertyNode)MemberwiseClone();
            // if copyChildren ...
            copy._childrenList = null;
            // if copyLabels ...
            copy._labels = null;

            if (XmlUtil.HasNilAttribute(copyNode))
            {
                copy.XsiNil = true;
            }

            // Swapping, so we don't want this.
            copy.Value = null;

            // Set the new node that's being used for the clone.
            copy.XmlNode = copyNode;

            // Apply new modification data based on the new node.
            copy.Version = XmlUtil.GetVersion(copyNode);
            copy.ModificationDate = XmlUtil.GetModificationDate(copyNode);

            return copy;
        }

        public PropertyNode ReplaceChild(PropertyNode newChild, PropertyNode originalChild)
        {
            Assert.IsNotNull(newChild);
            Assert.IsNotNull(originalChild);
            Assert.IsTrue(Children.Contains(originalChild));
            Assert.AreEqual(newChild.IContactName, originalChild.IContactName);

            int index = Children.IndexOf(originalChild);
            Assert.AreNotEqual(-1, index);

            Children[index] = newChild;

            return originalChild;
        }
    }
}
