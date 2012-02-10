/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using Standard;

    internal class SchematizedLabeledPropertyCollection<T> : ILabeledPropertyCollection<T>
    {
        public delegate T TypeCreator(Contact contact, string arrayNode);
        public delegate void TypeCommitter(Contact contact, string arrayNode, T value);

        private readonly Contact _contact;
        private readonly TypeCreator _creatorDelegate;
        private readonly TypeCommitter _committerDelegate;
        private readonly string _collection;
        private readonly string _createString;
        private readonly string[] _labelFilter;
        private readonly List<string> _nodeList;
        private bool _dirty;

        private void _OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var cpe = (ContactPropertyChangedEventArgs)e;

            // If the property changing isn't under this collection then we won't care.
            if (cpe.PropertyName.StartsWith(_collection, StringComparison.Ordinal))
            {
                _nodeList.Clear();
                _dirty = true;

                if (null != CollectionChanged)
                {
                    _EnsureNodeList();
                }
            }
        }

        private void _AugmentLabelFilters(ref string[] labels)
        {
            if (null == labels)
            {
                labels = _labelFilter;
                return;
            }

            if (null != _labelFilter)
            {
                var labelList = new List<string>(labels.Length + _labelFilter.Length);
                labelList.AddRange(_labelFilter);
                labelList.AddRange(labels);
                labels = labelList.ToArray();
            }
        }

        private string _GetIndexedProperty(int index)
        {
            _EnsureNodeList();
            return _nodeList[index];
        }

        private void _EnsureNodeList()
        {
            if (!_dirty)
            {
                return;
            }

            // TODO: Fix this.  The implementation is really quite nasty...
            // Not the most efficient implementation of INotifyCollectionChanged, but this should work.
            // Reset the collection and replace the full list once it's been enumerated.
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (null != handler)
            {
                handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            _nodeList.Clear();
            foreach (ContactProperty property in _contact.GetPropertyCollection(_collection, _labelFilter, false))
            {
                Assert.Implies(null != _labelFilter, property.PropertyType == ContactPropertyType.ArrayNode);
                if (ContactPropertyType.ArrayNode == property.PropertyType)
                {
                    _nodeList.Add(property.Name);
                }
            }

            if (null != handler)
            {
                var items = new List<T>();
                _nodeList.ForEach(propertyName => items.Add(_creatorDelegate(_contact, propertyName)));
                handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
            }

            _dirty = false;
        }

        public SchematizedLabeledPropertyCollection(Contact contact, string collectionType, string nodeName, TypeCreator creator, TypeCommitter committer)
            : this(contact, collectionType, nodeName, null, creator, committer)
        { }

        public SchematizedLabeledPropertyCollection(Contact contact, string collectionType, string nodeName, string labelFilter, TypeCreator creator, TypeCommitter committer)
        {
            Verify.IsNotNull(contact, "contact");
            Verify.IsNotNull(collectionType, "collectionType");
            Verify.IsNotNull(nodeName, "nodeName");
            Verify.IsNotNull(creator, "creator");

            var pni = new PropertyNameInfo(collectionType);
            switch (pni.Type)
            {
                case PropertyNameTypes.SchematizedCollectionName:
                    _collection = collectionType;
                    _createString = collectionType;
                    break;
                case PropertyNameTypes.SimpleExtensionCreationProperty:
                    _collection = "[" + pni.SimpleExtensionNamespace + "]" + pni.Level1;
                    _createString = collectionType;
                    break;
                default:
                    throw new ArgumentException("The provided string isn't a valid collection.  If you're trying to use simple extensions, use the create string format as the parameter.  E.g., \"[NS:Node]Collection\"", "collectionType");
            }

            _contact = contact;
            _contact.PropertyChanged += _OnPropertyChanged;

            _creatorDelegate = creator;
            _committerDelegate = committer;
            _nodeList = new List<string>();
            _dirty = true;

            if (null != labelFilter)
            {
                Assert.IsFalse(string.IsNullOrEmpty(labelFilter));
                _labelFilter = new[] { labelFilter };
            }
        }

        #region ILabeledPropertyCollection<T> Members

        public T Default
        {
            get
            {
                return this[(string[])null];
            }
            set
            {
                int index = DefaultIndex;
                if (-1 == index)
                {
                    this[(string[])null] = value;
                    index = DefaultIndex;
                    Assert.AreNotEqual(-1, index);
                }
                else
                {
                    this[index] = value;
                }
                DefaultIndex = index;
            }
        }

        public int DefaultIndex
        {
            get
            {
                _EnsureNodeList();
                string node = _contact.Properties.GetLabeledNode(_collection, _labelFilter);
                if (null == node)
                {
                    return -1;
                }

                int index = _nodeList.IndexOf(node);
                Assert.AreNotEqual(-1, index);
                return index;
            }
            set
            {
                for (int i = 0; i < Count; ++i)
                {
                    if (value == i)
                    {
                        GetLabelsAt(i).Add(PropertyLabels.Preferred);
                    }
                    else
                    {
                        GetLabelsAt(i).Remove(PropertyLabels.Preferred);
                    }
                }
            }
        }

        public ILabelCollection GetLabelsAt(int index)
        {
            return _contact.GetLabelCollection(_GetIndexedProperty(index));
        }

        public string GetNameAt(int index)
        {
            _EnsureNodeList();
            return _nodeList[index];
        }

        public string GetNameAt(params string[] labels)
        {
            return GetNameAt(IndexOfLabels(labels));
        }

        public int IndexOfLabels(params string[] labels)
        {
            _EnsureNodeList();
            _AugmentLabelFilters(ref labels);
            string node = _contact.Properties.GetLabeledNode(_collection, labels);
            if (null == node)
            {
                return -1;
            }

            int index = _nodeList.IndexOf(node);
            Assert.AreNotEqual(-1, index);
            return index;
        }

        private static string[] _MakeLabelArray(string label1, string label2, string label3)
        {
            int count = (null == label1 ? 0 : 1) + (null == label2 ? 0 : 1) + (null == label3 ? 0 : 1);
            string[] labels = null;
            if (0 != count)
            {
                labels = new string[count];
                if (null != label3)
                {
                    labels[--count] = label3;
                }
                if (null != label2)
                {
                    labels[--count] = label2;
                }
                if (null != label1)
                {
                    labels[--count] = label1;
                }
            }
            return labels;
        }

        public T this[string label]
        {
            get
            {
                return this[_MakeLabelArray(label, null, null)];
            }
            set
            {
                this[_MakeLabelArray(label, null, null)] = value;
            }
        }

        public T this[string label1, string label2]
        {
            get
            {
                return this[_MakeLabelArray(label1, label2, null)];
            }
            set
            {
                this[_MakeLabelArray(label1, label2, null)] = value;
            }
        }

        public T this[string label1, string label2, string label3]
        {
            get
            {
                return this[_MakeLabelArray(label1, label2, label3)];
            }
            set
            {
                this[_MakeLabelArray(label1, label2, label3)] = value;
            }
        }

        public T this[params string[] labels]
        {
            get
            {
                _AugmentLabelFilters(ref labels);
                string node = _contact.Properties.GetLabeledNode(_collection, labels);
                if (null == node)
                {
                    return default(T);
                }

                return _creatorDelegate(_contact, node);
            }
            set
            {
                _AugmentLabelFilters(ref labels);
                string node = _contact.Properties.GetLabeledNode(_collection, labels);
                if (null == node)
                {
                    node = _contact.AddNode(_createString, true);
                    if (null != labels && 0 != labels.Length)
                    {
                        try
                        {
                            _contact.GetLabelCollection(node).AddRange(labels);
                        }
                        catch
                        {
                            // Don't commit partial changes to the contact.
                            _contact.Invalidate();
                            throw;
                        }
                    }
                }

                // Now we have the node to work with.  Try to set the values.
                _committerDelegate(_contact, node, value);
            }
        }

        public void Add(T item, params string[] labels)
        {
            try
            {
                _AugmentLabelFilters(ref labels);
                string node = _contact.AddNode(_createString, true);
                if (null != labels)
                {
                    _contact.GetLabelCollection(node).AddRange(labels);
                }
                _committerDelegate(_contact, node, item);
            }
            catch
            {
                _contact.Invalidate();
                throw;
            }
        }

        #endregion

        #region IList<T> Members

        public int IndexOf(T item)
        {
            Predicate<T> doesItemMatch = t => null == t;
            if (null != item)
            {
                doesItemMatch = t => item.Equals(t);
            }

            int i = 0;
            foreach (T t in this)
            {
                if (doesItemMatch(t))
                {
                    return i;
                }
                ++i;
            }

            return -1;
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException("This interface doesn't support insertion of elements at arbitrary indexes");
        }

        public void RemoveAt(int index)
        {
            string property = _GetIndexedProperty(index);
            Assert.IsTrue(_contact.Properties.DoesPropertyExist(property));
            _contact.RemoveNode(property);
        }

        public T this[int index]
        {
            get
            {
                string property = _GetIndexedProperty(index);
                Assert.IsTrue(_contact.Properties.DoesPropertyExist(property));
                return _creatorDelegate(_contact, property);
            }
            set
            {
                string property = _GetIndexedProperty(index);
                Assert.IsTrue(_contact.Properties.DoesPropertyExist(property));
                _committerDelegate(_contact, property, value);
            }
        }
        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            Add(item, null);
        }

        public void Clear()
        {
            _EnsureNodeList();
            string[] nodeCopy = _nodeList.ToArray();
            foreach (string node in nodeCopy)
            {
                Assert.IsTrue(_contact.Properties.DoesPropertyExist(node));
                _contact.RemoveNode(node);
            }
        }

        public bool Contains(T item)
        {
            return -1 != IndexOf(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Verify.IsNotNull(array, "array");

            if (arrayIndex + Count >= array.Length)
            {
                throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, "The index implies that the copy should write past the length of the buffer.  The array hasn't been modified as part of this call.");
            }

            foreach (T item in this)
            {
                array[arrayIndex] = item;
                ++arrayIndex;
            }
        }

        public int Count
        {
            get
            {
                _EnsureNodeList();
                return _nodeList.Count;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            int i = IndexOf(item);
            if (-1 == i)
            {
                return false;
            }
            RemoveAt(i);
            return true;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            foreach (ContactProperty property in _contact.GetPropertyCollection(_collection, _labelFilter, false))
            {
                Assert.Implies(null != _labelFilter, ContactPropertyType.ArrayNode == property.PropertyType);
                if (ContactPropertyType.ArrayNode == property.PropertyType)
                {
                    yield return _creatorDelegate(_contact, property.Name);
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

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion
    }
}
