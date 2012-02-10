namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Standard;

    internal class LabelCollection : ILabelCollection
    {
        private readonly Contact _contact;
        private string _collection;
        private List<string> _labels;
        private bool _dirty;

        private static string _GetCollectionFromNode(string node)
        {
            Assert.IsTrue(PropertyNameUtil.IsPropertyValidNode(node));
            return node.Substring(0, node.LastIndexOf('/'));
        }

        private static string _GetFormatFromNode(string node)
        {
            Assert.IsTrue(PropertyNameUtil.IsPropertyValidNode(node));
            return node.Substring(0, node.LastIndexOf('[') + 1) + "{0}]";
        }

        private void _OnContactPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var cpc = (ContactPropertyChangedEventArgs)e;

            switch (cpc.ChangeType)
            {
                case ContactPropertyChangeType.NodeRemoved:
                    if (cpc.PropertyName == PropertyName)
                    {
                        // This is still valid, but re-request the label collection.
                        _dirty = true;
                    }
                    break;
                case ContactPropertyChangeType.NodePrepended:
                    if (null == _collection)
                    {
                        _collection = _GetCollectionFromNode(PropertyName);
                    }
                    if (_collection == _GetCollectionFromNode(cpc.PropertyName))
                    {
                        // Prepending a node into this collection, so just increment this index by one.
                        // The plus 2 is because GetIndexFromNode is 0 based and the XML node is 1 based
                        // (still only incrementing it by one).
                        PropertyName = string.Format(null, _GetFormatFromNode(PropertyName), PropertyNameUtil.GetIndexFromNode(PropertyName) + 2);
                        Assert.IsTrue(PropertyNameUtil.IsPropertyValidNode(PropertyName));
                        _dirty = true;
                    }
                    break;
                case ContactPropertyChangeType.LabelsAdded:
                case ContactPropertyChangeType.LabelsCleared:
                case ContactPropertyChangeType.LabelsRemoved:
                    // If there was another label collection that was used for the change then we need
                    // to requery the data.
                    if (cpc.PropertyName == PropertyName && sender != this)
                    {
                        _dirty = true;
                    }
                    break;
            }

        }

        public LabelCollection(Contact contact, string nodeName)
        {
            Assert.IsNotNull(contact);
            Assert.IsNotNull(nodeName);
            // Verify that the string is a valid node.
            if (!PropertyNameUtil.IsPropertyValidNode(nodeName))
            {
                throw new SchemaException(string.Format(null, "The arrayNode {0} doesn't appear to be valid", nodeName));
            }

            _contact = contact;
            PropertyName = nodeName;

            // Validate the argument
            if (!_contact.Properties.DoesPropertyExist(PropertyName))
            {
                throw new PropertyNotFoundException("The arrayNode doesn't exist in this contact.", nodeName);
            }

            // Load _labels on demand.

            // Use this to know whether _labels needs to be reloaded.
            _dirty = true;
            _contact.PropertyChanged += _OnContactPropertyChanged;
        }

        private void _RefreshLabels()
        {
            if (_dirty)
            {
                // Most implementations of GetLabels are probably using a List<string>.
                // If not, copy it so we can leverage functions written for List.
                IList<string> iLabels = _contact.Properties.GetLabels(PropertyName);
                _labels = iLabels as List<string> ?? new List<string>(iLabels);
                _dirty = false;
            }
        }

        #region ICollection<string> Members

        void ICollection<string>.Add(string item)
        {
            Add(item);
        }

        public void Clear()
        {
            _dirty = true;
            _contact.WriteableProperties.ClearLabels(PropertyName);

            // This should have a listener on the contact, so it really shouldn't be NULL.
            _contact.NotifyPropertyChanged(
                this,
                new ContactPropertyChangedEventArgs(
                    PropertyName,
                    ContactPropertyChangeType.LabelsRemoved));
        }

        public bool Contains(string item)
        {
            Verify.IsNeitherNullNorEmpty(item, "item");
            _RefreshLabels();

            return null != _labels.Find(label => string.Equals(item, label, StringComparison.OrdinalIgnoreCase));
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _RefreshLabels();
            _labels.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                _RefreshLabels();
                return _labels.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(string item)
        {
            Verify.IsNeitherNullNorEmpty(item, "item");
            _RefreshLabels();

            try
            {
                if (_contact.WriteableProperties.RemoveLabel(PropertyName, item))
                {
                    // Update our local copy with the same change.  No need to reset the _dirty bit.
                    // Need to do a case-insensitive removal here.
                    _labels.RemoveAll(label => string.Equals(item, label, StringComparison.OrdinalIgnoreCase));

                    // This should have a listener on the contact, so it really shouldn't be NULL.
                    _contact.NotifyPropertyChanged(
                        this,
                        new ContactPropertyChangedEventArgs(
                            PropertyName,
                            ContactPropertyChangeType.LabelsRemoved));

                    return true;
                }
            }
            catch
            {
                // Don't know how this failed.  Can't rely on our state.
                _dirty = true;
                throw;
            }

            return false;
        }

        #endregion

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            _RefreshLabels();
            return _labels.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ILabelCollection Members

        public string PropertyName { get; private set; }

        public bool Add(string item)
        {
            // Don't add duplicate labels to the contact.
            if (Contains(item))
            {
                return false;
            }

            _dirty = true;
            _contact.WriteableProperties.AddLabels(PropertyName, new[] { item });

            _contact.NotifyPropertyChanged(
                this,
                new ContactPropertyChangedEventArgs(
                    PropertyName,
                    ContactPropertyChangeType.LabelsAdded));
            return true;
        }

        public bool AddRange(params string[] items)
        {
            Verify.IsNotNull(items, "items");

            bool changed = false;
            foreach (string label in items)
            {
                if (Add(label))
                {
                    changed = true;
                }
            }
            return changed;
        }
        #endregion
    }

}
