/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

//#if USE_VISTA_WRITER

namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Windows.Resources;
    using System.Xml;
    using Interop;
    using Standard;
    using Standard.Interop;
    using Xml;
	using Microsoft.ContactsBridge.Interop;

    internal sealed class VistaContactProperties : IContactProperties, IDisposable
    {
        private INativeContactProperties _nativeContact;
        private Stream _streamCopy;
        // The native IContactProperties keeps a reference to this.  Can't drop it.
        private ManagedIStream _istreamCopy;
        private bool _modified;
        private string _streamHash;

        private static readonly Stream _EmptyContactStream;

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1804:RemoveUnusedLocals",
                MessageId = "app"),
            SuppressMessage(
                "Microsoft.Performance",
                "CA1810:InitializeReferenceTypeStaticFieldsInline",
                Justification="Need to explicitly call Application properties to ensure pack urls will work.")
        ]
        static VistaContactProperties()
        {
            // Using pack:// resources, so ensure that Uri won't throw an exception when not in a WPF app.
            #pragma warning disable 168
            var app = Application.Current;
            #pragma warning restore 168

            // Copy the emptycontact.xml contents into a local copy we can use for initialization.
            var emptyContactUri = new Uri(@"pack://application:,,,/" + ContactUtil.DllName + @";Component/Files/emptycontact.xml");
            _EmptyContactStream = new MemoryStream();
            StreamResourceInfo sri = Application.GetResourceStream(emptyContactUri);
            Assert.IsNotNull(sri);
            Utility.CopyStream(_EmptyContactStream, sri.Stream);
        }

        private ReadonlyContactProperties _CloneReadonly()
        {
            using (Stream stm = SaveToStream())
            {
                return new ReadonlyContactProperties(stm);
            }
        }

        private static Stream _GetNewContact()
        {
            // The emptycontact.xml provides a basic template for a new contact.
            // Need to replace template data in it with unique data, e.g. ContactID/Value and CreationDate,
            //   before trying to load it as a contact.

            // Could keep reusing the same XmlDocument, but would need to guard it for multiple-thread access.
            var xmlDoc = new XmlDocument();
            _EmptyContactStream.Position = 0;
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

        private void _Modify()
        {
            _modified = true;
            // Changed the file so we'll need to recalculate the hash if it's requested again.
            _streamHash = null;
        }

        private void _Validate()
        {
            if (null == _nativeContact)
            {
                throw new ObjectDisposedException("this");
            }
        }

        public static VistaContactProperties MakeWriteableCopy(IContactProperties properties)
        {
            Verify.IsNotNull(properties, "properties");
            Assert.IsTrue(properties.IsReadonly);

            // Can use the internal constructor so we don't unnecessarily dupe the stream.
            return new VistaContactProperties(properties.SaveToStream(), true);
        }

        public VistaContactProperties()
            : this(_GetNewContact(), true)
        {
            _modified = true;
        }

        public VistaContactProperties(Stream stream)
            : this(stream, false)
        {
        }

        private VistaContactProperties(Stream stream, bool ownsStream)
        {
            Verify.IsNotNull(stream, "stream");

            _nativeContact = new ContactRcw();

            var persistStream = (IPersistStream)_nativeContact;
            stream.Seek(0, SeekOrigin.Begin);

            // COM APIs may keep the stream and rely on using it later.
            // Managed callers are likely to call dispose on it, which destroys it.
            // Need to create a copy of the stream and keep it alive and unchanged
            // for the lifetime of this object.
            if (ownsStream)
            {
                // This is our own copy.  Don't need to dupe it.
                _streamCopy = stream;
            }
            else
            {
                _streamCopy = new MemoryStream();
                Utility.CopyStream(_streamCopy, stream);
            }
            _istreamCopy = new ManagedIStream(_streamCopy);

            HRESULT hr = persistStream.Load(_istreamCopy);
            if (HRESULT.WC_E_SYNTAX == hr || HRESULT.WC_E_GREATERTHAN == hr || Win32Error.ERROR_INVALID_DATATYPE == hr)
            {
                throw new InvalidDataException("The data stream is of an invalid format");
            }
            hr.ThrowIfFailed("An error occurred loading the contact");

            //_modified = false;
        }

        public string CreateArrayNode(string collectionName, bool appendNode)
        {
            _Modify();
            string nodeName;
            HRESULT hr = ContactUtil.CreateArrayNode(_nativeContact, collectionName, appendNode, out nodeName);
            if (Win32Error.ERROR_INVALID_DATATYPE == hr)
            {
                throw new SchemaException("Unable to create a new array arrayNode with " + collectionName);
            }
            hr.ThrowIfFailed("Unable to create a new array arrayNode with " + collectionName);
            return nodeName;
        }

        public bool DeleteArrayNode(string nodeName)
        {
            _Modify();

            HRESULT hr = ContactUtil.DeleteArrayNode(_nativeContact, nodeName);
            // Don't really care if the path is missing, we were removing it anyways.
            // Though we don't actually want to send a notification in that case.
            if (Win32Error.ERROR_PATH_NOT_FOUND != hr)
            {
                hr.ThrowIfFailed("Unable to delete the array arrayNode " + nodeName);
            }
            return hr.Succeeded();
        }

        public void ClearLabels(string node)
        {
            _Modify();
            ContactUtil.DeleteLabels(_nativeContact, node)
                .ThrowIfFailed("Unable to modify the labels on the arrayNode " + node);
        }

        public bool RemoveLabel(string propertyName, string label)
        {
            _Modify();
            List<string> currentLabels;

            // throws PropertyNotFoundException if the propertyName doesn't exist.
            ContactProperty nodeProp = GetAttributes(propertyName);
            if (nodeProp.PropertyType != ContactPropertyType.ArrayNode)
            {
                throw new ArgumentException(string.Format(null, "{0} is not an array node.", propertyName), "propertyName");
            }

            ContactUtil.GetLabels(_nativeContact, propertyName, out currentLabels).ThrowIfFailed();
            
            // Case insensitive search.
            List<string> copyLabels = currentLabels.FindAll(s => !string.Equals(label, s, StringComparison.OrdinalIgnoreCase));

            if (copyLabels.Count == currentLabels.Count)
            {
                // If the two lists are the same size then there's nothing being removed.
                return false;
            }

            // BADBAD: Can't make this atomic at this level.
            ContactUtil.DeleteLabels(_nativeContact, propertyName).ThrowIfFailed();
            if (0 != copyLabels.Count)
            {
                // Could try to restore if the SetLabels call fails, but there's no reason to expect
                // a second call with a bigger list would suddenly succeed.
                // There's no enforcement of label sets, so this shouldn't fail because of XSD validation.
                HRESULT hr = ContactUtil.SetLabels(_nativeContact, propertyName, copyLabels);
                Assert.IsTrue(hr.Succeeded());
                hr.ThrowIfFailed();
            }

            return true;
        }

        public bool DeleteProperty(string propertyName)
        {
            _Modify();
            HRESULT hr = ContactUtil.DeleteProperty(_nativeContact, propertyName);
            // Don't really care if the path is missing, we were removing it anyways.
            // Though we don't actually want to send a notification in that case.
            if (Win32Error.ERROR_PATH_NOT_FOUND == hr)
            {
                return false;
            }
            hr.ThrowIfFailed("Unable to remove the propertyName " + propertyName);
            return true;
        }

        public bool DoesPropertyExist(string property)
        {
            return ContactUtil.DoesPropertyExist(_nativeContact, property);
        }

        public ContactProperty GetAttributes(string propertyName)
        {
            // This isn't exposed by the native IContactProperties interface,
            // so do the same trick as GetPropertyCollection and get the value
            // from the managed ReadOnly implementation.
            using (ReadonlyContactProperties copyProperties = _CloneReadonly())
            {
                return copyProperties.GetAttributes(propertyName);
            }
        }

        public Stream GetBinary(string propertyName, out string propertyType)
        {
            string type;
            Stream stm;
            HRESULT hr = ContactUtil.GetBinary(_nativeContact, propertyName, true, out type, out stm);
            // If there's not a value at this property, just return null rather than throw
            // an exception.
            if (Win32Error.ERROR_PATH_NOT_FOUND == hr)
            {
				propertyType = null;
                return null;
            }
            // All errors other than a missing path warrant an exception.
            hr.ThrowIfFailed("Unable to get the binary value for " + propertyName);

            propertyType = type;

            return stm;
        }

        public DateTime? GetDate(string propertyName)
        {
            DateTime dt;
            HRESULT hr = ContactUtil.GetDate(_nativeContact, propertyName, true, out dt);
            // If there's not a value at this property, just return null rather than throw an exception.
            if (Win32Error.ERROR_PATH_NOT_FOUND == hr)
            {
                return null;
            }
            // All errors other than a missing path warrant an exception.
            hr.ThrowIfFailed("Unable to get the date value for " + propertyName);

            return dt;
        }

        public string GetLabeledNode(string collection, string[] labelFilter)
        {
            string node;
            HRESULT hr = ContactUtil.GetLabeledNode(_nativeContact, collection, labelFilter, out node);
            if (Win32Error.ERROR_PATH_NOT_FOUND == hr)
            {
                return null;
            }
            // If this failed for a reason other than that the node isn't present, throw the exception.
            hr.ThrowIfFailed("Unknown failure while searching for the preferred array arrayNode.");

            return node;
        }

        public IList<string> GetLabels(string node)
        {
            List<string> labels;
            ContactUtil.GetLabels(_nativeContact, node, out labels)
                .ThrowIfFailed("Unable to get the labels for the arrayNode " + node);
            return labels.AsReadOnly();
        }

        public IEnumerable<ContactProperty> GetPropertyCollection(string collectionName, string[] labelFilter, bool anyLabelMatches)
        {
            // This makes a copy of the contact and uses that for the property enumeration
            // because a bug in Windows causes contacts that have been modified in memory to
            // not properly walk simple extensions.  This is mitigated by saving the contact
            // to a stream and reloading it there.
            // CONSIDER: tracking whether the contact has been modified and only create this
            // copy when necessary.  Right now streamlining the implementation, if perf turns
            // into a problem because of this then revisit the decision.

            using (ReadonlyContactProperties copyProperties = _CloneReadonly())
            {
                // Don't use an iterator inside this function, so we can get the argument validation
                // that ReadonlyContactProperties.GetPropertyCollection gives us.
                IEnumerable<ContactProperty> enumerable = copyProperties.GetPropertyCollection(collectionName, labelFilter, anyLabelMatches);
                return _Iterate(enumerable);
            }
        }

        private IEnumerable<ContactProperty> _Iterate(IEnumerable<ContactProperty> enumerable)
        {
            foreach (var prop in enumerable)
            {
                // The copy's enumerator is only valid as long as this also is.
                _Validate();
                yield return prop;
            }
        }

        public string GetString(string propertyName)
        {
            string s;
            HRESULT hr = ContactUtil.GetString(_nativeContact, propertyName, true, out s);
            // If there's not a string at this property, just return null rather than throw
            // an exception.
            if (Win32Error.ERROR_PATH_NOT_FOUND == hr)
            {
                return "";
            }
            Assert.IsFalse(string.IsNullOrEmpty(s));
            // All errors other than a missing path warrant an exception.
            hr.ThrowIfFailed("Unable to get the string for " + propertyName);

            return s;
        }

        public bool IsReadonly
        {
            get
            {
                return false;
            }
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
            var memstm = new MemoryStream();
            using (var istream = new ManagedIStream(memstm))
            {
                ((IPersistStream)_nativeContact).Save(istream, true);
            }
            return memstm;
        }

        public void SetBinary(string propertyName, Stream value, string valueType)
        {
            _Modify();

            // content of valueType isn't validated.
            // ContactUtil.SetBinary doesn't wrap null valueTypes,
            // it will return E_INVALIDARG if passed NULL, so pass _something_.
            if (string.IsNullOrEmpty(valueType))
            {
                valueType = SchemaStrings.DefaultMimeType;
            }

            ContactUtil.SetBinary(_nativeContact, propertyName, valueType, value)
                .ThrowIfFailed("Unable to set the binary value for " + propertyName + ".");
        }

        public void SetDate(string propertyName, DateTime value)
        {
            _Modify();
            ContactUtil.SetDate(_nativeContact, propertyName, value)
                .ThrowIfFailed("Unable to set the string value for " + propertyName);
        }

        public void AddLabels(string node, ICollection<string> labels)
        {
            _Modify();
            ContactUtil.SetLabels(_nativeContact, node, labels)
                .ThrowIfFailed("Unable to modify the labels on the arrayNode " + node);
        }

        public void SetString(string propertyName, string value)
        {
            _Modify();
            HRESULT hr = ContactUtil.SetString(_nativeContact, propertyName, value);
            if (Win32Error.ERROR_INVALID_DATATYPE == hr)
            {
                throw new SchemaException("Unable to set the string value for " + propertyName + "because of schema.");
            }
            hr.ThrowIfFailed("Unable to set the string value for " + propertyName);
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

        #region IDisposable Members

        [
            SuppressMessage(
                "Microsoft.Usage",
                "CA2213:DisposableFieldsShouldBeDisposed",
                MessageId = "_istreamCopy"),
            SuppressMessage(
                "Microsoft.Usage",
                "CA2213:DisposableFieldsShouldBeDisposed",
                MessageId = "_streamCopy")
        ]
        public void Dispose()
        {
            Utility.SafeRelease(ref _nativeContact);
            Utility.SafeDispose(ref _streamCopy);
            Utility.SafeDispose(ref _istreamCopy);
        }

        #endregion
    }
}

//#endif
