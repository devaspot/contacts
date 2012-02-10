/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Xml;
    using System.Xml.Schema;
    using Standard;
    using Microsoft.Communications.Contacts.Xml;
	using Microsoft.ContactsBridge.Interop;

    internal class ReadonlyContactProperties : IContactProperties, IDisposable
    {
        /// <summary>A copy of the stream that this was loaded from.</summary>
        private Stream _sourceStream;
        /// <summary>The compiled Contact XSD.  Shared across all instances of this ContactProperties implementation.</summary>
        private static readonly XmlSchemaSet _contactSchemaCache;
        private readonly PropertyNodeDictionary _propertyLookup = new PropertyNodeDictionary();
        private readonly PropertyNode _contactRoot;
        private readonly PropertyNode _extendedRoot;
        private bool _disposed;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ReadonlyContactProperties()
        {
            _contactSchemaCache = new XmlSchemaSet();

            Uri contactXsdUri = ContactUtil.GetResourceUri("contact.xsd");
            var schema = XmlSchema.Read(Application.GetResourceStream(contactXsdUri).Stream, _OnValidationError);

            _contactSchemaCache.Add(schema);
            _contactSchemaCache.Compile();
        }

        private static void _OnValidationError(object sender, ValidationEventArgs e)
        {
            Assert.IsNotNull(e.Exception);
            throw e.Exception;
        }

        private static void _PopName(IList<string> nameStack, PropertyNode current)
        {
            if (0 != nameStack.Count && !SchemaStrings.SkipPropertyName(current.LocalName))
            {
                NodeTypes type = current.ContactNodeType;
                if (type == NodeTypes.ElementCollection || type == NodeTypes.ElementNode)
                {
                    string nodeName = current.LocalName;
                    if (type == NodeTypes.ElementNode)
                    {
                        nodeName += "[" + current.Parent.Children.Count.ToString("G", null) + "]";
                    }

                    if (nameStack[nameStack.Count - 1] == nodeName)
                    {
                        nameStack.RemoveAt(nameStack.Count - 1);
                    }
                }
            }
        }

        private static void _PushName(IList<string> nameStack, PropertyNode current)
        {
            if (!SchemaStrings.SkipPropertyName(current.LocalName))
            {
                NodeTypes type = current.ContactNodeType;
                if (type == NodeTypes.ElementCollection || type == NodeTypes.ElementNode)
                {
                    string nodeName = current.LocalName;
                    if (type == NodeTypes.ElementNode)
                    {
                        nodeName += "[" + current.Parent.Children.Count.ToString("G", null) + "]";
                        Assert.AreNotEqual(0, nameStack.Count);
                    }

                    if (nameStack.Count == 0 || nameStack[nameStack.Count - 1] != nodeName)
                    {
                        nameStack.Add(nodeName);
                    }
                }
            }
        }

        private void _Validate()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("this");
            }
        }

        public ReadonlyContactProperties(Stream source)
        {
            // _disposed = false;

            Assert.IsNotNull(source);
            _sourceStream = new MemoryStream();

            Utility.CopyStream(_sourceStream, source);

            var settings = new XmlReaderSettings
            {
                CheckCharacters = true,
                CloseInput = false,
                ConformanceLevel = ConformanceLevel.Document,
                IgnoreComments = true,
                IgnoreWhitespace = true,
                ValidationType = ValidationType.Schema
            };

            settings.Schemas.Add(_contactSchemaCache);
            settings.ValidationEventHandler += _OnValidationError;

            XmlReader reader = XmlReader.Create(_sourceStream, settings);
            DateTime? creationDate = null;

            try
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        // The root element should be "contact"
                        Assert.AreEqual(SchemaStrings.ContactRootElement, reader.LocalName);
                        Assert.IsTrue(PropertyNode.IsValidContactNamespace(reader.NamespaceURI));
                        // XSD should verify this attribute:
                        Assert.AreEqual("1", reader.GetAttribute(SchemaStrings.Version, SchemaStrings.ContactNamespace));
                        _contactRoot = new PropertyNode(reader, null);
                        break;
                    }
                }

                if (null == _contactRoot)
                {
                    throw new SchemaException("Missing root contact element");
                }

                // nodeStack contains the IContact names that lead to the current element..
                // It doesn't need to contain the root "contact" element, nor any non-IContactName elements inbetween.
                var nameStack = new List<string>();

                // XmlReader.Skip is awkward to use.  It positions itself after the close of the current element,
                // but doesn't return a boolean whether it's EOF.
                bool skipped = false;
                // Don't call Read after doing a Skip.  The Skip already moved the reader to the next element.
                for (PropertyNode current = _contactRoot; skipped ? !reader.EOF : reader.Read(); )
                {
                    skipped = false;
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Text:
                            // XML allows for adjacent Text nodes.  Not certain that the XSD blocks it...
                            Assert.IsNull(current.Value);
                            current.Value = reader.Value;
                            break;
                        case XmlNodeType.Element:

                            // Xsd should block us from ever seeing Label at this level
                            //    (it should only be seen inside a LabelCollection, inside ProcessLabels)
                            Assert.AreNotEqual(reader.LocalName, SchemaStrings.Label);
                            if (reader.LocalName == SchemaStrings.LabelCollection)
                            {
                                current.ProcessLabels(reader);
                                break;
                            }

                            // Only care about elements in supported namespaces.
                            if (!PropertyNode.IsValidContactNamespace(reader.NamespaceURI))
                            {
                                // This will move the reader past the closing element for this.
                                reader.Skip();
                                skipped = true;
                                continue;
                            }

                            var node = new PropertyNode(reader, current);

                            // The root SimpleExtension node is "Extended" underneath "contact"
                            if (node.LocalName == SchemaStrings.ExtensionsRootElement)
                            {
                                Assert.AreEqual(NodeTypes.RootElement, node.ContactNodeType);
                                if (current == _contactRoot)
                                {
                                    // The XSD prohibits multiple root "Extended" nodes.
                                    Assert.IsNull(_extendedRoot);
                                    _extendedRoot = node;
                                }
                                // Otherwise it's an escape mechanism to avoid schema.
                            }

                            // Add the current node to the IContactName list for the child element.
                            _PushName(nameStack, current);

                            current = node;

                            if (reader.IsEmptyElement)
                            {
                                goto case XmlNodeType.EndElement;
                            }
                            break;

                        case XmlNodeType.EndElement:

                            _PopName(nameStack, current);

                            Assert.AreEqual(current.LocalName, reader.LocalName);

                            // Generate IContact Name
                            var contactName = new StringBuilder();

                            contactName.Append(current.ExtendedNamespacePrefix);

                            foreach (string upName in nameStack)
                            {
                                contactName.Append(upName).Append("/");
                            }

                            contactName.Append(current.LocalName);

                            if (current.ContactNodeType == NodeTypes.ElementNode)
                            {
                                contactName.Append("[").Append(current.Parent.Children.Count.ToString("G", null)).Append("]");
                            }

                            current.IContactName = contactName.ToString();

                            if (PropertyNames.CreationDate == current.IContactName)
                            {
                                Assert.IsFalse(creationDate.HasValue);
                                creationDate = DateTime.Parse(current.Value, null, DateTimeStyles.AdjustToUniversal);
                                Assert.AreEqual(creationDate.Value.Kind, DateTimeKind.Utc);
                            }

                            // Move back to the parent element.
                            current = current.Parent;

                            break;
                    }
                }

                // XSD should catch this, yea?
                Assert.NullableIsNotNull(creationDate);

                foreach (PropertyNode enumNode in _contactRoot)
                { 
                    Assert.IsFalse(SchemaStrings.SkipPropertyName(enumNode.LocalName));
                    Assert.IsFalse(string.IsNullOrEmpty(enumNode.IContactName));
                    enumNode.EnsureVersionAndModificationDate(creationDate.Value);
                    _propertyLookup.Add(enumNode);
                }
            }
            catch (XmlException xmlEx)
            {
                throw new InvalidDataException("Invalid XML", xmlEx);
            }
        }

        #region IContactProperties Members

        #region Unsupported Modifying Methods

        string IContactProperties.CreateArrayNode(string collectionName, bool appendNode)
        {
            _Validate();
            throw new NotSupportedException();
        }

        bool IContactProperties.DeleteArrayNode(string nodeName)
        {
            _Validate();
            throw new NotSupportedException();
        }

        void IContactProperties.ClearLabels(string node)
        {
            _Validate();
            throw new NotSupportedException();
        }

        bool IContactProperties.RemoveLabel(string nodeName, string label)
        {
            _Validate();
            throw new NotSupportedException();
        }

        bool IContactProperties.DeleteProperty(string propertyName)
        {
            _Validate();
            throw new NotSupportedException();
        }

        void IContactProperties.SetBinary(string propertyName, Stream value, string valueType)
        {
            _Validate();
            throw new NotSupportedException();
        }

        void IContactProperties.SetDate(string propertyName, DateTime value)
        {
            _Validate();
            throw new NotSupportedException();
        }

        void IContactProperties.AddLabels(string node, ICollection<string> labels)
        {
            _Validate();
            throw new NotSupportedException();
        }

        void IContactProperties.SetString(string propertyName, string value)
        {
            _Validate();
            throw new NotSupportedException();
        }

        #endregion

        public bool DoesPropertyExist(string property)
        {
            _Validate();

            // Only array nodes should get nilled, so they still exist so far as this function is concerned.
            PropertyNode node = _propertyLookup.FindNode(property, NodeTypes.Any);
            // Internal interface, shouldn't be called like this
            Assert.Implies(null != node, () => node.ContactNodeType != NodeTypes.ElementCollection);

            return null != node;
        }

        public ContactProperty GetAttributes(string propertyName)
        {
            Verify.IsNeitherNullNorEmpty(propertyName, "propertyName");
            PropertyNode node = _propertyLookup.FindNode(propertyName, NodeTypes.Any);
            if (null == node)
            {
                throw new PropertyNotFoundException("The property was not found in this contact.", propertyName);                
            }

            if (node.ContactNodeType == NodeTypes.ElementCollection || node.ContactNodeType == NodeTypes.RootElement)
            {
                throw new SchemaException(propertyName + " is not a valid property for retrieving attributes.");
            }

            return new ContactProperty(node.IContactName, node.ContactPropertyType, node.Version, node.ElementId ?? default(Guid), node.ModificationDate, node.XsiNil);
        }

        public Stream GetBinary(string propertyName, out string propertyType)
        {
            propertyType = null;

            _Validate();

            Verify.IsNeitherNullNorEmpty(propertyName, "propertyName");

            PropertyNode node = _propertyLookup.FindNode(propertyName, NodeTypes.Any);
            if (null == node)
            {
                return null;
            }

            if (0 == node.ContentType.Length)
            {
                Assert.Fail();
                throw new SchemaException("Trying to get binary data but the target value doesn't contain a MIME type.");
            }

            // If the content existed but has been removed then return null.
            if (node.XsiNil)
            {
                return null;
            }

            byte[] byteData = Convert.FromBase64String(node.Value);

            Stream memStream = null;
            try
            {
                Stream retStream;

                memStream = new MemoryStream(byteData);

                propertyType = node.ContentType;
                retStream = memStream;
                memStream = null;
                return retStream;                
            }
            finally
            {
                Utility.SafeDispose(ref memStream);
            }
        }

        public DateTime? GetDate(string propertyName)
        {
            _Validate();

            Verify.IsNeitherNullNorEmpty(propertyName, "propertyName");

            PropertyNode node = _propertyLookup.FindNode(propertyName, NodeTypes.Any);
            if (null == _propertyLookup) 
            {
                return null;
            }

            DateTime retDate;
            if (!DateTime.TryParse(node.Value, null, DateTimeStyles.AdjustToUniversal, out retDate))
            {
                throw new SchemaException("Trying to get a date value but it doesn't appear to be properly formatted.");
            }

            return retDate;
        }

        public string GetLabeledNode(string collection, string[] labelFilter)
        {
            _Validate();

            Assert.IsFalse(string.IsNullOrEmpty(collection));

            if (null == labelFilter)
            {
                labelFilter = new string[0];
            }

            // Make a copy of the label set.
            // We're going to take two passes while trying to find the labeled value.
            // One has the Preferred label, the second doesn't.
            var preferredLabels = new string[labelFilter.Length + 1];
            labelFilter.CopyTo(preferredLabels, 0);
            preferredLabels[labelFilter.Length] = PropertyLabels.Preferred;

            IEnumerator<ContactProperty> enumerator = null;
            foreach (string[] filter in new [] { preferredLabels, labelFilter })
            {
                try
                {
                    enumerator = GetPropertyCollection(collection, filter, false).GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        return enumerator.Current.Name;
                    }
                }
                finally
                {
                    var disposable = enumerator as IDisposable;
                    Utility.SafeDispose(ref disposable);
                }
            }

            // If neither enumerator got a hit then we don't have a useful node.
            return null;
        }

        public IList<string> GetLabels(string node)
        {
            _Validate();

            Verify.IsNeitherNullNorEmpty(node, "node");

            PropertyNode propNode = _propertyLookup.FindNode(node, NodeTypes.Any);
            if (null == node)
            {
                return null;
            }

            return propNode.Labels;
        }

        public IEnumerable<ContactProperty> GetPropertyCollection(string collection, string[] labelFilter, bool anyLabelMatches)
        {
            _Validate();

            PropertyNode root = _contactRoot;

            // CONSIDER:
            // This should support indexed nodes as well as collections.
            if (null != collection)
            {
                root = _propertyLookup.FindNode(collection, NodeTypes.Any);
                if (null == root)
                {
                    // Collection doesn't exist.  No values to yield.
                    return Utility.GetEmptyEnumerable<ContactProperty>();
                }

                if (root.ContactNodeType != NodeTypes.ElementCollection)
                {
                    throw new ArgumentException("Not a collection", "collection");
                }

                // All collection nodes should have either the _extendedRoot or the _contactRoot as their parent.
                Assert.Implies(_contactRoot != root.Parent, _extendedRoot == root.Parent);
            }

            return _GetIterator(root, labelFilter, anyLabelMatches);
        }

        // Separate the GetEnumerator call from the iterator function so we can do argument validation
        // without requiring a call to MoveNext first.
        private IEnumerable<ContactProperty> _GetIterator(PropertyNode root, string[] labelFilter, bool anyLabelMatches)
        {
            foreach (PropertyNode node in root)
            {
                if (node.ContactNodeType != NodeTypes.ElementCollection && node.ContactNodeType != NodeTypes.RootElement)
                {
                    if (node.MatchesLabels(labelFilter, anyLabelMatches))
                    {
                        yield return new ContactProperty(node.IContactName, node.ContactPropertyType, node.Version, node.ElementId ?? default(Guid), node.ModificationDate, node.XsiNil);
                    }
                }
            }
        }

        public string GetString(string propertyName)
        {
            _Validate();

            Verify.IsNeitherNullNorEmpty(propertyName, "propertyName");

            PropertyNode node = _propertyLookup.FindNode(propertyName, NodeTypes.Any);
            if (null == node)
            {
                return "";
            }
            return node.Value ?? "";
        }

        public bool IsReadonly
        {
            get
            {
                _Validate();
                return true; 
            }
        }

        public bool IsUnchanged
        {
            get
            {
                _Validate();
                return true;
            }
        }

        public Stream SaveToStream()
        {
            _Validate();

            Stream copy = new MemoryStream();
            Utility.CopyStream(copy, _sourceStream);
            return copy;
        }

        public string StreamHash
        {
            get
            {
                _Validate();
                return Utility.HashStreamMD5(_sourceStream); 
            }
        }

        #endregion

        #region IDisposable Pattern

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_sourceStream")]
        public void Dispose()
        {
            _Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void _Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposed = true;
                Utility.SafeDispose(ref _sourceStream);
            }
        }

        #endregion
    }
}
