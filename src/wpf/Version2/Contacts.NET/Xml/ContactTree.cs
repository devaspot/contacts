namespace Microsoft.Communications.Contacts.Xml
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using Standard;

    // TODO:
    // Additional verifications:
    //     * All ElementIDs are unique.
    //     * SimpleExtension nodes are the same name for a collection.
    //     * SimpleExtension nodes children with the same name are also the same type.
    //     * SimpleExtension XmlNodes all have the c:Type attribute, and it is correct.
    //     * Verify that a collection node only exists in the tree once.
    //     * Verify that labels only exist in the label collection once.
    //     * Verify that no empty nodes exist in the label collection.
    //     * Verify simple extension binary properties have mime-types.
    //     * (Maybe) verify that no labels vary simply by case?
    // Once we start doing this, need to convert ReadonlyContactProperties to also use ContactTree
    //     so we can keep the validation consistent and localized.
    //     Should wrap the XmlDocument in an XmlReader subclass that exposes the XmlNode if it's available.
    //     Can then consolidate the two parsers.
    internal class ContactTree : IEquatable<ContactTree>
    {
        public PropertyNodeDictionary Lookup { get; private set; }
        public PropertyNode ContactRoot { get; private set; }
        public PropertyNode ExtendedRoot { get; private set; }
        public DateTime CreationDate { get; private set; }

        public ContactTree(XmlDocument document)
        {
            // The root element should be "contact"
            Assert.AreEqual(SchemaStrings.ContactRootElement, document.DocumentElement.LocalName);
            Assert.IsTrue(PropertyNode.IsValidContactNamespace(document.DocumentElement.NamespaceURI));

            Lookup = new PropertyNodeDictionary();

            ContactRoot = new PropertyNode(document.DocumentElement, null);

            Assert.IsTrue(document.DocumentElement.HasChildNodes);

            foreach (XmlNode childNode in document.DocumentElement.ChildNodes)
            {
                _BuildTree(ContactRoot, childNode);
            }

            // XSD should catch these.
            Assert.IsNotNull(ExtendedRoot);
            Assert.AreNotEqual(default(DateTime), CreationDate);

            foreach (PropertyNode enumNode in ContactRoot)
            {
                Assert.IsFalse(SchemaStrings.SkipPropertyName(enumNode.LocalName));
                Assert.IsNeitherNullNorEmpty(enumNode.IContactName);
                enumNode.EnsureVersionAndModificationDate(CreationDate);
                Lookup.Add(enumNode);
            }

            // Ensure that there's a modification date on the root element.
            ContactRoot.EnsureVersionAndModificationDate(CreationDate);
        }

        private void _BuildTree(PropertyNode parentProperty, XmlNode currentNode)
        {
            PropertyNode dontCare;
            _BuildTree(parentProperty, currentNode, out dontCare);
        }

        private void _BuildTree(PropertyNode parentProperty, XmlNode currentNode, out PropertyNode newProperty)
        {
            newProperty = null;
            switch (currentNode.NodeType)
            {
                case XmlNodeType.Text:
                    Assert.IsNull(parentProperty.Value);
                    parentProperty.Value = currentNode.Value;
                    return;
                case XmlNodeType.Element:
                    // Xsd should block us from ever seeing Label at this level
                    //    (it should only be seen inside a LabelCollection, inside ProcessLabels)
                    Assert.AreNotEqual(currentNode.LocalName, SchemaStrings.Label);

                    // Only care about elements in supported namespaces.
                    if (!PropertyNode.IsValidContactNamespace(currentNode.NamespaceURI))
                    {
                        return;
                    }

                    if (currentNode.LocalName == SchemaStrings.LabelCollection)
                    {
                        parentProperty.ProcessLabels(currentNode);

                        // Just processed all children of this node that we care to.
                        return;
                    }

                    newProperty = new PropertyNode(currentNode, parentProperty);

                    // The root SimpleExtension node is "Extended" underneath "contact"
                    if (newProperty.LocalName == SchemaStrings.ExtensionsRootElement)
                    {
                        Assert.AreEqual(NodeTypes.RootElement, newProperty.ContactNodeType);
                        if (parentProperty == ContactRoot)
                        {
                            // The XSD should block multiple root "Extended" nodes.
                            // (it also should block there not being one present)
                            Assert.IsNull(ExtendedRoot);
                            ExtendedRoot = newProperty;
                        }
                        // Otherwise it's an escape mechanism to avoid schema.
                        // The extended root doesn't have additional properties, but we still need to process children.
                    }
                    else
                    {
                        // Generate IContact Name
                        var contactName = new StringBuilder();

                        if (!parentProperty.IContactName.StartsWith(newProperty.ExtendedNamespacePrefix, StringComparison.Ordinal))
                        {
                            contactName.Append(newProperty.ExtendedNamespacePrefix);
                        }

                        if (!string.IsNullOrEmpty(parentProperty.IContactName))
                        {
                            contactName.Append(parentProperty.IContactName).Append("/");
                        }

                        contactName.Append(newProperty.LocalName);

                        if (newProperty.ContactNodeType == NodeTypes.ElementNode)
                        {
                            contactName.Append("[")
                                       .Append(parentProperty.Children.Count.ToString("G", null))
                                       .Append("]");
                        }

                        newProperty.IContactName = contactName.ToString();
                    }

                    if (currentNode.HasChildNodes)
                    {
                        foreach (XmlNode childNode in currentNode.ChildNodes)
                        {
                            _BuildTree(newProperty, childNode);
                        }
                    }

                    if (PropertyNames.CreationDate == newProperty.IContactName)
                    {
                        Assert.AreEqual(default(DateTime), CreationDate);
                        Assert.IsNotNull(newProperty.Value);
                        CreationDate = DateTime.Parse(newProperty.Value, null, DateTimeStyles.AdjustToUniversal);
                        Assert.AreEqual(CreationDate.Kind, DateTimeKind.Utc);
                    }

                    return;
            }
            // Each of the switch branches should have explicitly returned.
            Assert.Fail();
        }

        #region Object overrides

        public override bool Equals(object obj)
        {
            var otherTree = obj as ContactTree;
            if (null == otherTree)
            {
                return false;
            }
            return Equals(otherTree);
        }

        public override int GetHashCode()
        {
            Assert.IsNotNull(ContactRoot);
            return ContactRoot.GetHashCode();
        }

        #endregion

        #region IEquatable<ContactTree> Members

        public bool Equals(ContactTree other)
        {
            if (null == other)
            {
                return false;
            }

            if (!ContactRoot.Equals(other.ContactRoot))
            {
                return false;
            }

            return true;
        }

        #endregion

        [System.Diagnostics.Conditional("DEBUG")]
        internal void AssertEquals(ContactTree contactTree)
        {
            Assert.IsNotNull(contactTree);

            ContactRoot.AssertEquals(contactTree.ContactRoot);
        }
    }
}
