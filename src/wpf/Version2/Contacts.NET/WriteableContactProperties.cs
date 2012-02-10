/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

// #define DEBUGGING_CONTACT_XML

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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

    // Class notes:
    // * Generally throw SchemaExceptions rather than ArgumentExceptions when attempting incorrect modifications to
    //     the contact.
    // * Prefer throwing PropertyNotFoundException to returning null when the choice exists.
    // * PNode suffix refers to PropertyNodes.
    // * XNode suffix refers to XmlNodes.

    /// <summary>
    /// IContactProperties implementation that supports modification.
    /// </summary>
    internal sealed class WriteableContactProperties : IContactProperties, IDisposable
    {
        // Until we have a reference to System.Core (.Net 3.5)...
        private delegate void Action();
        private delegate TResult Func<TResult>();

        /// <summary>The compiled Contact XSD.  Shared across all instances of this ContactProperties implementation.</summary>
        private static readonly XmlSchemaSet _ContactSchemaCache;
        private static readonly Stream _EmptyContactStream;

        private readonly ContactTree _contactTree;
        private readonly XmlElementManager _namespaceManager;
        private readonly XmlDocument _document;

        /// <summary>
        /// Flag to indicate that the object has been disposed,
        /// or has performed some operation with partial success and left the object in an unknown state.
        /// </summary>
        private bool _unusable;

        /// <summary>
        /// Whether the DOM has been changed since this was created.
        /// </summary>
        private bool _modified;
        private string _streamHash;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static WriteableContactProperties()
        {
            Uri contactXsdUri = ContactUtil.GetResourceUri("contact.xsd");
            XmlSchema schema = XmlSchema.Read(Application.GetResourceStream(contactXsdUri).Stream, _OnValidationError);

            _ContactSchemaCache = new XmlSchemaSet();
            _ContactSchemaCache.Add(schema);
            _ContactSchemaCache.Compile();

            // Copy the emptycontact.xml contents into a local copy we can use for initialization.
            Uri emptyContactUri = ContactUtil.GetResourceUri("emptycontact.xml");
            _EmptyContactStream = new MemoryStream();
            Stream s = Application.GetResourceStream(emptyContactUri).Stream;
            Utility.CopyStream(_EmptyContactStream, s);
        }

        [Conditional("DEBUGGING_CONTACT_XML")]
        private static void _SuperExpensiveDeepValidate(WriteableContactProperties source)
        {
            using (WriteableContactProperties clone = MakeWriteableCopy(source))
            {
                clone._contactTree.AssertEquals(source._contactTree);
            }
        }

        /// <summary>
        /// Generate a new, unique Contact with a minimimal set of default data.
        /// </summary>
        /// <returns>A new contact with a unique Id and a CreationDate of the time of the call.</returns>
        private static Stream _GetNewContact()
        {
            // The emptycontact.xml provides a basic template for a new contact.
            // Need to replace template data in it with unique data, e.g. ContactID/Value and CreationDate,
            //   before trying to load it as a contact.

            _EmptyContactStream.Position = 0;

            // Could keep reusing the same XmlDocument, but would need to guard it for multiple-thread access.
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(_EmptyContactStream);

            var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("c", SchemaStrings.ContactNamespace);
            Assert.AreEqual(SchemaStrings.ContactNamespace, nsmgr.LookupNamespace("c"));

            var contactIdElement = xmlDoc.SelectSingleNode("./c:contact/c:ContactIDCollection/c:ContactID", nsmgr) as XmlElement;
            contactIdElement.SetAttribute(SchemaStrings.ElementId, SchemaStrings.ContactNamespace, Guid.NewGuid().ToString());
            var valueElement = contactIdElement.FirstChild as XmlElement;
            valueElement.InnerText = Guid.NewGuid().ToString();

            var creationDateElement = xmlDoc.SelectSingleNode("./c:contact/c:CreationDate", nsmgr) as XmlElement;
            creationDateElement.InnerText = XmlUtil.DateTimeNowString;

            Stream stm = new MemoryStream();
            xmlDoc.Save(stm);

            return stm;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void _SetUnusableOnException(Action action, Func<bool> rollback)
        {
            Assert.IsNotNull(action);
            Assert.IsFalse(_unusable);
            try
            {
                action();
            }
            catch
            {
                // Messed up the tree.  Need to rollback the operations to keep the object usable.

                // Try to rollback.  If it succeeds then we're good, but we'll still throw the outer Exception.
                bool stillUnusable = true;
                if (null != rollback)
                {
                    try
                    {
                        if (rollback())
                        {
                            stillUnusable = false;
                        }
                    }
                    catch
                    {
                        // Swallow any exceptions that occur in a rollback.
                    }
                }

                _unusable = stillUnusable;

                // rethrow the exception that caused this.
                throw;
            }
        }

        /// <summary>
        /// Standard handler for XML validation errors.  Throws the exception as it was seen.
        /// </summary>
        /// <param name="sender">Sender of the error.</param>
        /// <param name="e">EventArgs information about the error.</param>
        private static void _OnValidationError(object sender, ValidationEventArgs e)
        {
            Assert.IsNotNull(e.Exception);
            throw e.Exception;
        }

        /// <summary>Set internal fields to signal that the object is being modified.</summary>
        private void _Modify()
        {
            _modified = true;
            // Changed the file so we'll need to recalculate the hash if it's requested again.
            _streamHash = null;
        }

        public WriteableContactProperties()
            : this(_GetNewContact(), true)
        {
            // New contact is considered a modification.
            _Modify();
        }

        public WriteableContactProperties(Stream source)
            : this(source, false)
        {}
        
        public WriteableContactProperties(Stream source, bool ownsStream)
        {
            // _disposed = false;
            // _modified = false;

            Assert.IsNotNull(source);

            try
            {
                _document = new XmlDocument();
                _document.Schemas.Add(_ContactSchemaCache);

                source.Position = 0;
                _document.Load(source);

                if (ownsStream)
                {
                    Utility.SafeDispose(ref source);
                }

                // Coalesce adjacent Text nodes.  The XSD may make this superfluous.
                _document.Normalize();

                _ValidateDom();

                _contactTree = new ContactTree(_document);
                _namespaceManager = new XmlElementManager(_document);
            }
            catch (XmlException xmlEx)
            {
                throw new InvalidDataException("Invalid XML", xmlEx);
            }
        }

        public static WriteableContactProperties MakeWriteableCopy(IContactProperties properties)
        {
            Verify.IsNotNull(properties, "properties");

            // Can use the internal constructor so we don't unnecessarily dupe the stream.
            return new WriteableContactProperties(properties.SaveToStream(), true);
        }

        private void _Validate()
        {
            if (_unusable)
            {
                throw new ObjectDisposedException("this");
            }

            Assert.Evaluate(_ValidateDom);
        }

        private void _ValidateDom()
        {
            _document.Validate((sender, args) => { throw new SchemaException(args.Message, args.Exception); });

            if (!_IsValidContactIdCollection())
            {
                throw new SchemaException("ContactIDCollection doesn't satisfy contact requirements.");
            }
        }

        /// <summary>
        /// Basic verification of the ContactId collection contracts.
        /// </summary>
        /// <remarks>
        /// This is used to verify the DOM before committing changes to the other data structures.
        /// It can only use the DOM to verify.
        /// 
        /// This isn't enforced in the XSD (because it's hard to express) but it's required for contacts:
        /// * There must be a ContactIDCollection (enforced by XSD)
        /// * There must be at least one ContactID child (enforced by XSD)
        /// * There must be at least one Value child with a GUID set (this function)
        /// </remarks>
        private bool _IsValidContactIdCollection()
        {
            // Not using LINQ because don't want to require .Net 3.5.
            foreach (XmlNode collectionXNode in _document.DocumentElement.ChildNodes)
            {
                if (collectionXNode.NamespaceURI == SchemaStrings.ContactNamespace
                    && collectionXNode.LocalName == "ContactIDCollection")
                {
                    foreach (XmlNode idXNode in collectionXNode.ChildNodes)
                    {
                        if (idXNode.NamespaceURI == SchemaStrings.ContactNamespace
                            && idXNode.LocalName == "ContactID")
                        {
                            foreach (XmlNode valueXNode in idXNode.ChildNodes)
                            {
                                if (valueXNode.NamespaceURI == SchemaStrings.ContactNamespace
                                    && valueXNode.LocalName == "Value")
                                {
                                    string s = valueXNode.InnerText;
                                    Guid id;
                                    if (!string.IsNullOrEmpty(s) && Utility.GuidTryParse(s, out id))
                                    {
                                        return true;
                                    }

                                    break;
                                }
                                break;
                            }
                        }
                    }
                }
            }

            // ContactIDCollection doesn't satisfy contact requirements.
            return false;
        }

        #region IContactProperties Members

        #region Helper functions for schematized properties

        /// <summary>
        /// Creates a new array node for a schematized collection.  Also creates the collection if necessary.
        /// </summary>
        /// <param name="collectionName">The collection to create the new node in.</param>
        /// <param name="appendNode">Whether to append or prepend the new node in the collection.</param>
        /// <returns>The name of the new node.</returns>
        /// <remarks>
        /// The array node name is inferred since this is only used for known collections.
        /// </remarks>
        private string _CreateSchematizedArrayNode(string collectionName, bool appendNode)
        {
            string arrayNodeName = SchemaStrings.TryGetCollectionNodeName(collectionName);
            if (string.IsNullOrEmpty(arrayNodeName))
            {
                throw new SchemaException(collectionName + " is not a valid collection name.");
            }

            PropertyNode newPNode = null;

            // About to start modifying the DOM tree.
            // If we fail we need to successfully roll back any changes to keep the object usable.
            _SetUnusableOnException(
                () =>
                {
                    // TODO: To successfully rollback here we need to know whether the collection was added,
                    //    since an empty collection will generally violate the XSD.
                    PropertyNode collectionPNode = _EnsureCollection(collectionName);

                    XmlElement arrayXNode = _namespaceManager.CreateSchemaElement(arrayNodeName);

                    XmlAttribute elementIdAttribute = _namespaceManager.CreateElementIdAttribute();
                    elementIdAttribute.Value = Guid.NewGuid().ToString();
                    arrayXNode.Attributes.Append(elementIdAttribute);
                    _namespaceManager.AddNilAttribute(arrayXNode);
                    _namespaceManager.UpdateVersionAndModificationDate(arrayXNode);
                    

                    if (appendNode)
                    {
                        collectionPNode.XmlNode.AppendChild(arrayXNode);
                    }
                    else
                    {
                        collectionPNode.XmlNode.PrependChild(arrayXNode);
                    }

                    // This function shouldn't be able to do anything that causes an invalid document.
                    Assert.Evaluate(_ValidateDom);

                    // ??? _contactTree.AdjustTree
                    newPNode = new PropertyNode(arrayXNode, collectionPNode, appendNode)
                    {
                        Version = 1
                    };
                    _FixupChildren(collectionPNode, arrayNodeName);
                },
                null);
            
            Assert.Evaluate(() => _document.Validate((sender, e) => Assert.Fail()));
            _SuperExpensiveDeepValidate(this);

            Assert.IsNeitherNullNorEmpty(newPNode.IContactName);
            return newPNode.IContactName;
        }

        private PropertyNode _EnsureCollection(string collectionName)
        {
            Assert.IsNeitherNullNorEmpty(collectionName);
            Assert.IsNeitherNullNorEmpty(SchemaStrings.TryGetCollectionNodeName(collectionName));

            PropertyNode propNode = _contactTree.Lookup.FindNode(collectionName, NodeTypes.Any);
            if (null != propNode)
            {
                return propNode;
            }

            // All collection nodes are children of the root contact node.
            XmlElement collectionElement = _namespaceManager.CreateSchemaElement(collectionName);

            // Insert the node into the DOM tree.
            // This is only going to be called when creating an array node
            //    under it, so don't need to set the nil attribute.
            _document.DocumentElement.AppendChild(collectionElement);

            // Don't validate, because the empty collection might be illegal.
            // This should only be called when we're about to add a node,
            // so the validation will occur in a short while.

            // Also insert the node into the PropertyNode mirror.
            // ??? _contactTree.AddXmlNode(collectionElement, _contactTree.ContactRoot);
            propNode = new PropertyNode(collectionElement, _contactTree.ContactRoot)
            {
                Version = 1,
                IContactName = collectionName
            };

            _contactTree.Lookup.Add(propNode);

            return propNode;
        }

        #endregion

        #region Helper functions for simple extension properties

        private string _CreateSimpleArrayNode(string collectionName, bool appendNode)
        {
            Assert.IsNeitherNullNorEmpty(collectionName);
            Assert.AreEqual('[', collectionName[0]);

            var token = new PropertyNameInfo(collectionName);
            if (token.Type != PropertyNameTypes.SimpleExtensionCreationProperty)
            {
                throw new ArgumentException("The property name is improperly formatted for creating a new simple extension node.");
            }

            Assert.IsNeitherNullNorEmpty(token.Level1);
            Assert.IsNeitherNullNorEmpty(token.Level2);
            Assert.IsNeitherNullNorEmpty(token.SimpleExtensionNamespace);

            PropertyNode collectionNode = _EnsureSimpleCollection(token.SimpleExtensionNamespace, token.Level1);
            string xmlPrefix = collectionNode.ExtendedNamespacePrefix.Substring(1, collectionNode.ExtendedNamespacePrefix.Length - 2);
            XmlElement nodeElement = _document.CreateElement(xmlPrefix, token.Level2, collectionNode.ExtendedNamespace);

            // New nodes are set to nil.  Version is implicitly "1".
            XmlAttribute elementIdAttribute = _namespaceManager.CreateElementIdAttribute();
            elementIdAttribute.Value = Guid.NewGuid().ToString();
            XmlAttribute typeAttribute = _namespaceManager.CreateNodeTypeAttribute(SchemaStrings.SchemaTypeArrayNode);

            nodeElement.Attributes.Append(elementIdAttribute);
            nodeElement.Attributes.Append(typeAttribute);

            _namespaceManager.AddNilAttribute(nodeElement);
            _namespaceManager.UpdateVersionAndModificationDate(nodeElement);

            PropertyNode newNode = null;
            // Now modifying the DOM (probably modified it by creating the collection earlier, too...).
            _SetUnusableOnException(
                () =>
                {
                    if (appendNode)
                    {
                        collectionNode.XmlNode.AppendChild(nodeElement);
                    }
                    else
                    {
                        collectionNode.XmlNode.PrependChild(nodeElement);
                    }

                    // ??? _contactTree.AdjustTree();
                    newNode = new PropertyNode(nodeElement, collectionNode, appendNode) {Version = 1};
                    _FixupChildren(collectionNode, token.Level2);

                },
                null);

            _SuperExpensiveDeepValidate(this);
            
            Assert.IsNeitherNullNorEmpty(newNode.IContactName);
            return newNode.IContactName;
        }

        private PropertyNode _EnsureSimpleCollection(string extensionNamespace, string collectionName)
        {
            Assert.IsNeitherNullNorEmpty(extensionNamespace);
            Assert.IsNeitherNullNorEmpty(collectionName);

            string icontactPath = "[" + extensionNamespace + "]" + collectionName;
            PropertyNode propNode = _contactTree.Lookup.FindNode(icontactPath, NodeTypes.Any);
            if (null != propNode)
            {
                return propNode;
            }

            // All simple extension collection nodes are children of the root extension node.
            XmlElement collectionElement = _namespaceManager.CreateExtensionElement(extensionNamespace, collectionName);

            collectionElement.Attributes.Append(_namespaceManager.CreateNodeTypeAttribute(SchemaStrings.SchemaTypeArrayElement));

            _namespaceManager.AddNilAttribute(collectionElement);

            bool removedExtendedRootNil = false;

            _SetUnusableOnException(
                () =>
                {
                    // Adding a child node to extended, ensure it doesn't have the xsi:nil attribute.
                    if (_contactTree.ExtendedRoot.XsiNil)
                    {
                        Assert.AreEqual(0, _contactTree.ExtendedRoot.Children.Count);
                        XmlUtil.RemoveNilAttribute(_contactTree.ExtendedRoot.XmlNode);
                        _contactTree.ExtendedRoot.XsiNil = false;
                        // Need to rollback this action if we fail.
                        removedExtendedRootNil = true;
                    }

                    // Insert the node into the DOM tree.
                    _contactTree.ExtendedRoot.XmlNode.AppendChild(collectionElement);

                    // Also insert the node into the PropertyNode mirror.
                    propNode = new PropertyNode(collectionElement, _contactTree.ExtendedRoot)
                    {
                        Version = 1,
                        IContactName = icontactPath
                    };

                    _contactTree.Lookup.Add(propNode);
                },
                () =>
                {
                    // This was an append, so it should be LastChild.
                    XmlNode parentNode = _contactTree.ExtendedRoot.XmlNode;
                    if (parentNode.LastChild == collectionElement)
                    {
                        parentNode.RemoveChild(collectionElement);
                    }

                    // Remove the propNode from the tree.
                    if (null != propNode)
                    {
                        _contactTree.ExtendedRoot.Children.Remove(propNode);
                        _contactTree.Lookup.Remove(propNode);
                    }

                    // If this was the first extended collection being added, might need to add back the Nil attribute.
                    if (removedExtendedRootNil)
                    {
                        _namespaceManager.AddNilAttribute(_contactTree.ExtendedRoot.XmlNode);
                        _contactTree.ExtendedRoot.XsiNil = true;
                    }

                    return true;
                });

            return propNode;
        }

        #endregion

        /// <summary>
        /// Corrects the IContact naming of element nodes.  Call this after modifying a collection's children.
        /// </summary>
        /// <param name="collectionNode">The collection node that needs its children fixed up.</param>
        /// <param name="nodeName">The name to use for the children nodes.</param>
        private void _FixupChildren(PropertyNode collectionNode, string nodeName)
        {
            Assert.IsNotNull(collectionNode);
            Assert.IsNeitherNullNorEmpty(nodeName);

            // First pass, remove everything from the lookup table and update the names.
            int printIndex = 1;
            foreach (PropertyNode child in collectionNode.Children)
            {
                // Only expecting this to be used to correct indices.
                Assert.IsTrue(
                    string.IsNullOrEmpty(child.IContactName)
                    || (child.IContactName.StartsWith(collectionNode.IContactName + "/" + nodeName, StringComparison.Ordinal)
                        && child.IContactName.EndsWith("]", StringComparison.Ordinal)));
                string newName = string.Format(null, "{0}/{1}[{2}]", collectionNode.IContactName, nodeName, printIndex);
                _contactTree.Lookup.Remove(child);
                child.IContactName = newName;
                ++printIndex;
            }
            // Second pass, add back to the lookup table with the correct names.
            foreach (PropertyNode child in collectionNode.Children)
            {
                _contactTree.Lookup.Add(child);
            }
        }

        public string CreateArrayNode(string collectionName, bool appendNode)
        {
            _Validate();
            _Modify();

            Verify.IsNeitherNullNorEmpty(collectionName, "collectionName");

            if ('[' == collectionName[0])
            {
                return _CreateSimpleArrayNode(collectionName, appendNode);
            }

            return _CreateSchematizedArrayNode(collectionName, appendNode);
        }

        // BUGBUG: Need to remove ContentType attribute when deleting binary properties.
        private bool _DeleteProperty(PropertyNode oldProperty)
        {
            Assert.IsNotNull(oldProperty.Parent);

            // Make a copy to work with.
            // Since we're setting xsi:nil, don't need to make a deep copy.
            XmlNode copyNode = oldProperty.XmlNode.CloneNode(false);

            _namespaceManager.AddNilAttribute(copyNode);
            _namespaceManager.UpdateVersionAndModificationDate(copyNode);

            XmlNode originalParentNode = oldProperty.XmlNode.ParentNode;
            XmlNode originalNode = oldProperty.XmlNode;

            bool swappedNodes = false;
            bool swappedProperties = false;
            PropertyNode copyProperty = null;
            _SetUnusableOnException(
                () =>
                {
                    // Modify the DOM tree and validate the new version.  If this fails we can still roll back.
                    originalParentNode.ReplaceChild(copyNode, originalNode);

                    swappedNodes = true;

                    _ValidateDom();

                    copyProperty = oldProperty.CloneForSwap(false, false, copyNode);

                    // Change looks good.  Start modifying the rest of the class-level data structures.
                    // Failure here isn't handled.  We'll instead keep the invalidate flag on the object.
                    copyProperty.Parent.ReplaceChild(copyProperty, oldProperty);

                    swappedProperties = true;

                    _contactTree.Lookup.Remove(oldProperty, true);
                    _contactTree.Lookup.Add(copyProperty);
                },
                () =>
                {
                    if (swappedNodes)
                    {
                        originalParentNode.ReplaceChild(originalNode, copyNode);
                    }

                    if (swappedProperties)
                    {
                        Assert.IsNotNull(copyProperty);
                        copyProperty.Parent.ReplaceChild(oldProperty, copyProperty);
                    }

                    _contactTree.Lookup.Remove(copyProperty);
                    foreach (PropertyNode readdProperties in oldProperty)
                    {
                        _contactTree.Lookup.Replace(readdProperties);
                    }

                    return true;
                });

            Assert.Evaluate(() => _document.Validate((sender, e) => Assert.Fail()));
            _SuperExpensiveDeepValidate(this);

            return true;
        }

        public bool DeleteArrayNode(string nodeName)
        {
            _Validate();
            _Modify();

            Verify.IsNeitherNullNorEmpty(nodeName, "nodeName");

            PropertyNode node = _contactTree.Lookup.FindNode(nodeName, NodeTypes.Any);
            if (null == node)
            {
                return false;
            }

            if (node.ContactPropertyType != ContactPropertyType.ArrayNode)
            {
                throw new ArgumentException("The property is of the wrong type.  This function only supports ArrayNodes.", "nodeName");
            }

            return _DeleteProperty(node);
        }

        public void ClearLabels(string nodeName)
        {
            _Validate();
            _Modify();

            Verify.IsNeitherNullNorEmpty(nodeName, "nodeName");

            PropertyNode arrayProperty = _contactTree.Lookup.FindNode(nodeName, NodeTypes.Any);
            if (null == arrayProperty)
            {
                throw new PropertyNotFoundException("The node doesn't exist to have its labels removed.", nodeName);
            }

            if (arrayProperty.ContactPropertyType != ContactPropertyType.ArrayNode)
            {
                throw new ArgumentException("The property isn't an ArrayNode, and can't have labels to remove.", "nodeName");
            }

            if (0 == arrayProperty.Labels.Count)
            {
                // It's an array node, but it doesn't have any labels to remove.
                // Don't do unnecessary work, bail early.
                return;
            }

            XmlNode labelCollectionNode = _FindLabelCollection(arrayProperty.XmlNode);
            Assert.IsNotNull(labelCollectionNode);
            
            XmlNode labelCollectionCopy = labelCollectionNode.CloneNode(false);
            // Clear the node.  It doesn't have modification data, so no worries about what to keep.
            labelCollectionCopy.RemoveAll();
            _namespaceManager.AddNilAttribute(labelCollectionCopy);

            _SetUnusableOnException(
                () =>
                {
                    arrayProperty.XmlNode.ReplaceChild(labelCollectionCopy, labelCollectionNode);
                    arrayProperty.ProcessLabels(labelCollectionCopy);
                },
                null);

            _SuperExpensiveDeepValidate(this);
        }

        private static XmlNode _FindLabelCollection(XmlNode arrayNode)
        {
            foreach (XmlNode childNode in arrayNode.ChildNodes)
            {
                if (childNode.LocalName == SchemaStrings.LabelCollection
                    && childNode.NamespaceURI == SchemaStrings.ContactNamespace)
                {
                    return childNode;
                }
            }

            return null;
        }

        public bool DeleteProperty(string propertyName)
        {
            _Validate();
            _Modify();

            PropertyNode node = _contactTree.Lookup.FindNode(propertyName, NodeTypes.Any);
            if (null == node)
            {
                return false;
            }

            // Note that this check makes it so the caller is allowed to delete collection nodes with this call.
            // Might want to block this behavior, but right now I'd rather have the freedom to be strict with
            // the SetString calls, and the caller could otherwise get themselves into a state where they can't
            // change the type of a simple extension node.
            if (node.ContactPropertyType == ContactPropertyType.ArrayNode)
            {
                throw new ArgumentException("The property is of the wrong type.  This function doesn't support ArrayNodes..", "propertyName");
            }

            return _DeleteProperty(node);
        }

        public void SetBinary(string propertyName, Stream value, string valueType)
        {
            _Validate();
            _Modify();

            Verify.IsNeitherNullNorEmpty(propertyName, "propertyName");
            Verify.IsNotNull(value, "value");

            if (string.IsNullOrEmpty(valueType))
            {
                valueType = SchemaStrings.DefaultMimeType;
            }

            string valueB64;
            using (var stm = new MemoryStream())
            {
                Utility.CopyStream(stm, value);
                // Didn't want to do this on the original stream in case it would throw.
                if (stm.Length == 0)
                {
                    throw new ArgumentException("Stream cannot be empty.", "value");
                }

                valueB64 = Convert.ToBase64String(stm.GetBuffer(), 0, (int)stm.Length, Base64FormattingOptions.None);
            }

            _SetString(propertyName, valueB64, valueType, SchemaStrings.SchemaTypeBinary);
        }

        public void SetDate(string propertyName, DateTime value)
        {
            _Validate();
            _Modify();

            Verify.IsNeitherNullNorEmpty(propertyName, "propertyName");

            // If the caller hasn't explicitly set the kind then assume it's UTC
            // so it will be written as read to the Contact.  
            if (value.Kind != DateTimeKind.Local)
            {
                value = new DateTime(value.Ticks, DateTimeKind.Utc);
            }

            string dtValue = value.ToUniversalTime().ToString("s", null) + "Z";

            _SetString(propertyName, dtValue, null, SchemaStrings.SchemaTypeDate);
        }

        public void AddLabels(string nodeName, ICollection<string> labels)
        {
            _Validate();
            _Modify();

            Verify.IsNeitherNullNorEmpty(nodeName, "nodeName");
            Verify.IsNotNull(labels, "labels");

            PropertyNode arrayProperty = _contactTree.Lookup.FindNode(nodeName, NodeTypes.Any);
            if (null == arrayProperty)
            {
                throw new PropertyNotFoundException("The node doesn't exist to have labels added to it.", nodeName);
            }

            if (arrayProperty.ContactPropertyType != ContactPropertyType.ArrayNode)
            {
                throw new ArgumentException("The property isn't an ArrayNode, and can't have labels.", "nodeName");
            }

            XmlNode labelCollectionNode = _FindLabelCollection(arrayProperty.XmlNode);
            XmlNode labelCollectionCopy;

            if (null == labelCollectionNode)
            {
                labelCollectionCopy = _namespaceManager.CreateSchemaElement(SchemaStrings.LabelCollection);
                // Don't bother adding the Nil attribute.  We're certainly adding to the collection.
                // Don't add it to the tree yet.  Modify the DOM as atomically as possible.
            }
            else
            {
                labelCollectionCopy = labelCollectionNode.CloneNode(true);
                XmlUtil.RemoveNilAttribute(labelCollectionCopy);
            }

            _AddToLabelCollection(labelCollectionCopy, labels);

            bool setNode = false;
            _SetUnusableOnException(
                () =>
                {
                    if (null == labelCollectionNode)
                    {
                        // Ensure that the xsi:Nil isn't present.
                        XmlUtil.RemoveNilAttribute(arrayProperty.XmlNode);
                        arrayProperty.XsiNil = false;
                        arrayProperty.XmlNode.AppendChild(labelCollectionCopy);
                    }
                    else
                    {
                        arrayProperty.XmlNode.ReplaceChild(labelCollectionCopy, labelCollectionNode);
                    }

                    setNode = true;

                    arrayProperty.ProcessLabels(labelCollectionCopy);
                },
                () =>
                {
                    if (setNode)
                    {
                        if (null == labelCollectionNode)
                        {
                            arrayProperty.XmlNode.RemoveChild(labelCollectionCopy);
                        }
                        else
                        {
                            arrayProperty.XmlNode.ReplaceChild(labelCollectionNode, labelCollectionCopy);
                        }

                        arrayProperty.ProcessLabels(labelCollectionNode);
                    }
                    return true;
                });

            Assert.Evaluate(_ValidateDom);
        }

        private void _AddToLabelCollection(XmlNode labelCollectionNode, IEnumerable<string> labels)
        {
            // Labels are really URIs, so if we can't convert everything being added to one then this isn't going to succeed.
            foreach (string foreachLabel in labels)
            {
                // local swap because I can't modify the iteration variable reference.
                string label = foreachLabel.Trim();
                if (string.IsNullOrEmpty(label))
                {
                    throw new ArgumentException("One of the items is an invalid labels since it is null or empty.", "labels");
                }

                Uri url;
                if (!Uri.TryCreate(label, UriKind.RelativeOrAbsolute, out url))
                {
                    throw new ArgumentException("One of the provided labels isn't a valid URL.", "labels");
                }

                // Verify that the collection doesn't already contain this label.
                foreach (XmlNode childNode in labelCollectionNode.ChildNodes)
                {
                    Assert.AreEqual(childNode.LocalName, SchemaStrings.Label);
                    if (string.Equals(childNode.InnerXml, label, StringComparison.OrdinalIgnoreCase))
                    {
                        // It's already present, so skip this.
                        continue;
                    }
                }

                // Add this node to the copy of the collection.
                XmlNode labelNode = _namespaceManager.CreateSchemaElement(SchemaStrings.Label);
                labelNode.InnerText = label;
                labelCollectionNode.AppendChild(labelNode);
            }
        }

        public bool RemoveLabel(string nodeName, string label)
        {
            _Validate();
            _Modify();

            Verify.IsNeitherNullNorEmpty(nodeName, "nodeName");
            Verify.IsNeitherNullNorEmpty(label, "label");

            PropertyNode arrayProperty = _contactTree.Lookup.FindNode(nodeName, NodeTypes.Any);
            if (null == arrayProperty)
            {
                throw new PropertyNotFoundException("The node doesn't exist in the contact.", nodeName);
            }

            XmlNode labelCollectionNode = _FindLabelCollection(arrayProperty.XmlNode);
            if (null == labelCollectionNode)
            {
                // No labels to remove.
                return false;
            }

            XmlNode labelCollectionCopy = labelCollectionNode.CloneNode(true);

            if (!_RemoveFromLabelCollection(labelCollectionCopy, label))
            {
                return false;
            }

            bool setNode = false;
            _SetUnusableOnException(
                () =>
                {
                    arrayProperty.XmlNode.ReplaceChild(labelCollectionCopy, labelCollectionNode);
                    setNode = true;
                    arrayProperty.ProcessLabels(labelCollectionCopy);
                },
                () =>
                {
                    if (setNode)
                    {
                        arrayProperty.XmlNode.ReplaceChild(labelCollectionNode, labelCollectionCopy);
                        arrayProperty.ProcessLabels(labelCollectionNode);
                    }
                    return true;
                });

            Assert.Evaluate(_ValidateDom);

            return true;
        }

        private static bool _RemoveFromLabelCollection(XmlNode labelCollectionNode, string label)
        {
            label = label.Trim();
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("The label is invalid since it is null or empty.", "label");
            }

            // Don't need to verify URI-ness of the label.  If it's present, remove it.

            // Do need to make sure we remove redundant instances of the label.
            List<XmlNode> labelsToRemove = new List<XmlNode>();
            foreach (XmlNode childNode in labelCollectionNode.ChildNodes)
            {
                Assert.AreEqual(childNode.LocalName, "Label");
                if (string.Equals(childNode.InnerXml, label, StringComparison.OrdinalIgnoreCase))
                {
                    labelsToRemove.Add(childNode);
                }
            }

            if (0 == labelsToRemove.Count)
            {
                return false;
            }

            labelsToRemove.ForEach(node => labelCollectionNode.RemoveChild(node));

            return true;
        }

        public void SetString(string propertyName, string value)
        {
            _Validate();
            _Modify();

            Verify.IsNeitherNullNorEmpty(propertyName, "propertyName");
            Verify.IsNeitherNullNorEmpty(value, "value");

            _SetString(propertyName, value, null, SchemaStrings.SchemaTypeString);
        }

        // TODO: Need to write the schema type on simple extensions.
        // TODO: Need to add valueType attribute for binary data.
        private void _SetString(string propertyName, string value, string valueType, string schemaType)
        {
            Assert.IsNeitherNullNorEmpty(propertyName);
            Assert.IsNeitherNullNorEmpty(value);
            Assert.IsNeitherNullNorEmpty(schemaType);
            Assert.Implies(schemaType == SchemaStrings.SchemaTypeBinary, !string.IsNullOrEmpty(valueType));
            Assert.Implies(schemaType != SchemaStrings.SchemaTypeBinary, null == valueType);

            var token = new PropertyNameInfo(propertyName);

            PropertyNode parentProperty;
            string elementName;
            string nodeName;

            switch (token.Type)
            {
                case PropertyNameTypes.SchematizedCollectionName:
                case PropertyNameTypes.SimpleExtensionNode:
                case PropertyNameTypes.SchematizedNode:
                case PropertyNameTypes.SimpleExtensionCreationProperty:
                    // None of these types of properties can have values set on them.
                    throw new ArgumentException("This type of property cannot have a value set to it.");

                case PropertyNameTypes.SchematizedHierarchicalProperty:
                    // If it's a hierarchical property, need to make sure that the parent node already exists.
                    nodeName = token.Level1 + "/" + token.Level2 + "[" + token.Index + "]";
                    parentProperty = _contactTree.Lookup.FindNode(nodeName, NodeTypes.Any);
                    if (null == parentProperty)
                    {
                        throw new PropertyNotFoundException("The node where the property is to be set doesn't exist.  The node needs to be created first.", nodeName);
                    }
                    elementName = token.Level3;
                    break;

                case PropertyNameTypes.SimpleExtensionHierarchicalProperty:
                    // If it's a hierarchical property, need to make sure that the parent node already exists.
                    nodeName = "[" + token.SimpleExtensionNamespace + "]" + token.Level1 + "/" + token.Level2 + "[" + token.Index + "]";
                    parentProperty = _contactTree.Lookup.FindNode(nodeName, NodeTypes.Any);
                    if (null == parentProperty)
                    {
                        throw new PropertyNotFoundException("The node where the property is to be set doesn't exist.  The node needs to be created first.", nodeName);
                    }
                    elementName = token.Level3;
                    break;

                case PropertyNameTypes.SchematizedTopLevelProperty:
                    // Nothing needs to be done for schematized top level properties.  Parent is the root Contact node.
                    parentProperty = _contactTree.ContactRoot;
                    elementName = token.Level1;
                    break;

                case PropertyNameTypes.SimpleExtensionTopLevel:
                    // the root is just the extension root.

                    parentProperty = _contactTree.ExtendedRoot;
                    elementName = token.Level1;
                    break;

                default:
                    Assert.Fail();
                    throw new ArgumentException("Invalid property name.", "propertyName");
            }

            Assert.IsNotNull(parentProperty);
            Assert.Implies(parentProperty.ContactNodeType != NodeTypes.ElementNode, parentProperty.ContactNodeType == NodeTypes.RootElement);

            // See if the node we're trying to write to already exists.
            // Otherwise we'll need to create it (and rollback its creation on failure).
            PropertyNode oldProperty = null;
            foreach (PropertyNode child in parentProperty.Children)
            {
                if (child.IContactName == propertyName)
                {
                    oldProperty = child;

                    // For simple extensions, make sure that if this is a top level property
                    if (child.ContactNodeType == NodeTypes.ElementCollection)
                    {
                        throw new ArgumentException("This property represents a collection and doesn't support having values set on it.");
                    }
                    // I think with the current implementation the XML could have been explicitly manipulated to violate this.
                    // Might need to make the conditions around this assert a bit more bullet-proof.
                    Assert.AreEqual(0, child.Children.Count);
                    break;
                }
            }

            XmlNode childNode;
            if (null == oldProperty)
            {
                if (string.IsNullOrEmpty(token.SimpleExtensionNamespace))
                {
                    childNode = _namespaceManager.CreateSchemaElement(elementName);
                }
                else
                {
                    childNode = _namespaceManager.CreateExtensionElement(token.SimpleExtensionNamespace, elementName);
                }
            }
            else
            {
                childNode = oldProperty.XmlNode.CloneNode(false);
                XmlUtil.RemoveNilAttribute(childNode);
            }

            _namespaceManager.UpdateVersionAndModificationDate(childNode);
            XmlText valueText = _document.CreateTextNode(value);
            childNode.AppendChild(valueText);

            if (schemaType == SchemaStrings.SchemaTypeBinary)
            {
                _namespaceManager.SetMimeTypeAttribute(childNode, valueType);
            }

            if (!string.IsNullOrEmpty(token.SimpleExtensionNamespace))
            {
                _namespaceManager.SetSchemaTypeAttribute(childNode, schemaType);
            }

            //bool wasParentNil = false;
            _SetUnusableOnException(
                () =>
                {
                    if (parentProperty.XsiNil)
                    {
                        //wasParentNil = true;
                        XmlUtil.RemoveNilAttribute(parentProperty.XmlNode);
                        parentProperty.XsiNil = false;
                    }
        
                    PropertyNode childProperty;
                    if (null == oldProperty)
                    {
                        parentProperty.XmlNode.PrependChild(childNode);

                        // will need to remove this from parentProperty if we fail.
                        childProperty = new PropertyNode(childNode, parentProperty, false, value, propertyName);
                    }
                    else
                    {
                        parentProperty.XmlNode.ReplaceChild(childNode, oldProperty.XmlNode);
                        //PropertyNode.RebuildSubtree(childNode, oldProperty);
                        childProperty = oldProperty.CloneForSwap(false, false, childNode);
                        childProperty.Value = value;

                        parentProperty.ReplaceChild(childProperty, oldProperty);
                    }

                    _ValidateDom();

                    _contactTree.Lookup.Replace(childProperty);

                    _SuperExpensiveDeepValidate(this);
                },
                null);
        }

        public bool DoesPropertyExist(string property)
        {
            _Validate();
            // Only array nodes should get nilled, so they still exist so far as this function is concerned.
            PropertyNode node = _contactTree.Lookup.FindNode(property, NodeTypes.Any);
            Assert.Implies(node != null, () => node.ContactNodeType != NodeTypes.ElementCollection);
            return null != node;
        }

        public ContactProperty GetAttributes(string propertyName)
        {
            _Validate();

            Verify.IsNeitherNullNorEmpty(propertyName, "propertyName");
            PropertyNode node = _contactTree.Lookup.FindNode(propertyName, NodeTypes.Any);
            if (null == node)
            {
                throw new PropertyNotFoundException(propertyName + " was not found in this contact.", propertyName);
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

            PropertyNode node = _contactTree.Lookup.FindNode(propertyName, NodeTypes.Any);
            if (null == node)
            {
                return null;
            }

            if (0 == node.ContentType.Length)
            {
                // The XSD should have caught this if it was a schematized binary property.
                // Simple extensions should have been verified when the tree was built.
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

            PropertyNode node = _contactTree.Lookup.FindNode(propertyName, NodeTypes.Any);
            if (null == node)
            {
                return null;
            }

            if (node.XsiNil)
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

            Assert.IsNeitherNullNorEmpty(collection);

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
            foreach (string[] filter in new[] { preferredLabels, labelFilter })
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

            PropertyNode propNode = _contactTree.Lookup.FindNode(node, NodeTypes.Any);
            if (null == propNode)
            {
                return null;
            }

            return propNode.Labels;
        }

        public IEnumerable<ContactProperty> GetPropertyCollection(string collection, string[] labelFilter, bool anyLabelMatches)
        {
            _Validate();

            PropertyNode root = _contactTree.ContactRoot;

            if (null != collection)
            {
                root = _contactTree.Lookup.FindNode(collection, NodeTypes.Any);
                if (null == root)
                {
                    // Collection doesn't exist.  No values to yield.
                    return Utility.GetEmptyEnumerable<ContactProperty>();
                }

                if (root.ContactNodeType != NodeTypes.ElementCollection)
                {
                    throw new ArgumentException("Not a collection", "collection");
                }

                // All collection nodes should have either the _contactTree.ExtendedRoot or the _contactTree.ContactRoot as their parent.
                Assert.Implies(_contactTree.ContactRoot != root.Parent, _contactTree.ExtendedRoot == root.Parent);
            }

            // Guard against changes to the tree while enumerating.
            // Build a list of ContactPropertys with the current data and yield on that.
            var properties = new List<ContactProperty>();
            foreach (PropertyNode node in root)
            {
                if (node.ContactNodeType != NodeTypes.ElementCollection && node.ContactNodeType != NodeTypes.RootElement)
                {
                    if (node.MatchesLabels(labelFilter, anyLabelMatches))
                    {
                        properties.Add(new ContactProperty(node.IContactName, node.ContactPropertyType, node.Version, node.ElementId ?? default(Guid), node.ModificationDate, node.XsiNil));
                    }
                }
            }

            return _Iterate(properties);
        }

        private IEnumerable<ContactProperty> _Iterate(IEnumerable<ContactProperty> enumerable)
        {
            foreach (var prop in enumerable)
            {
                _Validate();
                yield return prop;
            }
        }

        public string GetString(string propertyName)
        {
            _Validate();

            Verify.IsNeitherNullNorEmpty(propertyName, "propertyName");

            PropertyNode node = _contactTree.Lookup.FindNode(propertyName, NodeTypes.Any);
            if (null == node)
            {
                return "";
            }
            return node.Value ?? "";
        }

        public bool IsReadonly
        {
            get { return false; }
        }

        public bool IsUnchanged
        {
            get 
            {
                _Validate();
                return !_modified; 
            }
        }

        public Stream SaveToStream()
        {
            _Validate();
            var settings = new XmlWriterSettings
            {
                CloseOutput = false,
                Encoding = Encoding.UTF8,
                Indent = true,
                NewLineHandling = NewLineHandling.Entitize,
                NewLineOnAttributes = false,
                OmitXmlDeclaration = false,
            };
            Stream copy = new MemoryStream();
            using (XmlWriter writer = XmlWriter.Create(copy, settings))
            {
                _document.Save(writer);
            }
            copy.Position = 0;
            return copy;
        }

        public string StreamHash
        {
            get
            {
                if (null == _streamHash)
                {
                    using (Stream stm = SaveToStream())
                    {
                        _streamHash = Utility.HashStreamMD5(stm);
                    }
                }
                return _streamHash;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void _Dispose(bool disposing)
        {
            _unusable = true;
            if (disposing)
            { }
        }

        #endregion
    }
}
