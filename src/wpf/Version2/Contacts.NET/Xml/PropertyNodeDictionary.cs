namespace Microsoft.Communications.Contacts.Xml
{
    using System;
    using System.Collections.Generic;
    using Standard;

    internal class PropertyNodeDictionary : IEquatable<PropertyNodeDictionary>
    {
        private readonly Dictionary<string, PropertyNode> _propertyLookup;

        public PropertyNodeDictionary()
        {
            _propertyLookup = new Dictionary<string, PropertyNode>();
        }

        public PropertyNode FindNode(string collectionName, NodeTypes filter)
        {
            PropertyNode outNode;
            if (_propertyLookup.TryGetValue(collectionName, out outNode))
            {
                if ((outNode.ContactNodeType & filter) != NodeTypes.None)
                {
                    return outNode;
                }
            }
            return null;
        }

        // Similar to Add except doesn't throw if the item already exists.
        public void Replace(PropertyNode node)
        {
            Assert.IsNotNull(node);
            Assert.IsNeitherNullNorEmpty(node.IContactName);

            _propertyLookup[node.IContactName] = node;
        }

        public void Add(PropertyNode node)
        {
            Assert.IsNotNull(node);
            Assert.IsNeitherNullNorEmpty(node.IContactName);

            _propertyLookup.Add(node.IContactName, node);
        }

        public void Remove(PropertyNode node)
        {
            Assert.IsNotNull(node);
            if (!string.IsNullOrEmpty(node.IContactName))
            {
                _propertyLookup.Remove(node.IContactName);
            }
        }

        public void Remove(PropertyNode node, bool recurse)
        {
            Assert.IsNotNull(node);

            if (!recurse)
            {
                Remove(node);
            }
            else
            {
                foreach (PropertyNode child in node)
                {
                    Remove(child);
                }
            }
        }

        private int _Count
        {
            get
            {
                return _propertyLookup.Count;
            }
        }

        #region Object overrides

        public override bool Equals(object obj)
        {
            var other = obj as PropertyNodeDictionary;
            if (null == other)
            {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return _propertyLookup.GetHashCode();
        }

        #endregion

        #region IEquatable<PropertyNodeDictionary> Members

        public bool Equals(PropertyNodeDictionary other)
        {
            // Going to walk the dictionary to determine equality.
            // If they're not the same size, they're not equal.
            if (other._Count != _Count)
            {
                return false;
            }

            foreach (var pair in _propertyLookup)
            {
                PropertyNode otherValue;
                if (!other._propertyLookup.TryGetValue(pair.Key, out otherValue))
                {
                    return false;
                }

                if (!pair.Value.Equals(otherValue))
                {
                    return false;
                }

                Assert.AreEqual(pair.Key, pair.Value.IContactName);
                Assert.AreEqual(pair.Key, otherValue.IContactName);
            }

            return true;
        }

        #endregion
    }
}
