/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

#if USE_VISTA_WRITER
namespace Microsoft.Communications.Contacts.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;
    using Standard;
    using Standard.Interop;

    // Disambiguate System.Runtime.InteropServices.FILETIME
    using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

    internal sealed class MarshalableLabelCollection : IDisposable
    {
        // The managed array of LPCWSTRs as IntPtrs.
        private readonly IntPtr[] _nativeStrings;
        // The buffer that contains the marhshalable version of the LPCWSTRs.
        [SuppressMessage(
            "Microsoft.Reliability",
            "CA2006:UseSafeHandleToEncapsulateNativeResources",
            Justification = "Tracked by WorkItem 4522")]
        private IntPtr _nativeArray;
        // Number of LPCWSTRs allocated in _nativeStrings.
        // In case of partial object creation, this is needed for accurate cleanup.
        private readonly uint _count;

        public MarshalableLabelCollection(ICollection<string> labels)
        {
            // _count = 0;
            // _nativeArray = IntPtr.Zero;
            // _nativeStrings = null;

            if (null != labels)
            {
                // This doesn't need to be greater than zero.
                // If this represents a 0 length array, the handle returned is NULL.
                if (labels.Count > 0)
                {
                    // Since we're allocating memory, be ready to cleanup if this throws at any point.
                    try
                    {
                        _nativeStrings = new IntPtr[labels.Count];
                        foreach (string label in labels)
                        {
                            if (string.IsNullOrEmpty(label))
                            {
                                throw new SchemaException("The array must not contain empty strings");
                            }
                            _nativeStrings[_count++] = Marshal.StringToCoTaskMemUni(label);
                        }

                        _nativeArray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(IntPtr)) * _nativeStrings.Length);
                        Marshal.Copy(_nativeStrings, 0, _nativeArray, _nativeStrings.Length);
                    }
                    catch
                    {
                        // Something happened: probably either an argument or out of memory exception.
                        // The finalizer would get called, but it's better to clean up our own mess.
                        _Dispose();
                        throw;
                    }
                }
            }
        }

        public IntPtr MarshaledLabels
        {
            get
            {
                return _nativeArray;
            }
        }

        public uint Count
        {
            get
            {
                return _count;
            }
        }

        #region IDisposable Pattern

        public void Dispose()
        {
            _Dispose();
            GC.SuppressFinalize(this);
        }

        ~MarshalableLabelCollection()
        {
            _Dispose();
        }

        private void _Dispose()
        {
            Utility.SafeCoTaskMemFree(ref _nativeArray);

            // If there's a count of strings, then there must be an array where they are stored.
            Assert.Implies(_count > 0, null != _nativeStrings);

            for (int i = 0; i < _count; ++i)
            {
                Utility.SafeCoTaskMemFree(ref _nativeStrings[i]);
            }
        }

        #endregion
    }

    internal sealed class MarshalableDoubleNullString : IDisposable
    {
        [SuppressMessage(
            "Microsoft.Reliability",
            "CA2006:UseSafeHandleToEncapsulateNativeResources",
            Justification="Tracked by WorkItem 4522")]
        private IntPtr _buffer;

        public MarshalableDoubleNullString(uint characterCapacity)
        {
            Realloc(characterCapacity);
        }

        public uint Capacity { get; private set; }

        public IntPtr MarshaledString
        {
            get
            {
                return _buffer;
            }
        }

        public List<string> ParsedStrings
        {
            get
            {
                return InteropUtil.ParseDoubleNullString(_buffer);
            }
        }

        public void Realloc(uint cch)
        {
            if (0 == cch)
            {
                throw new ArgumentException("Can't allocate zero-sized native buffer");
            }
            _Dispose();
            _buffer = Marshal.AllocCoTaskMem((int)(cch * Win32Value.sizeof_WCHAR));
            Capacity = cch;
        }

        #region IDisposable Pattern

        public void Dispose()
        {
            _Dispose();
            GC.SuppressFinalize(this);
        }

        ~MarshalableDoubleNullString()
        {
            _Dispose();
        }

        private void _Dispose()
        {
            Utility.SafeCoTaskMemFree(ref _buffer);
        }

        #endregion
    }

    /// <summary>
    /// Static utility class to ease working with the native COM IContact interfaces.
    /// </summary>
    internal static class InteropUtil
    {
        private static DateTime DateTimeFromFILETIME(FILETIME ft)
        {
            ulong l = (uint)ft.dwHighDateTime;
            l <<= 32;
            l |= (uint)ft.dwLowDateTime;
            DateTime dt = DateTime.FromFileTimeUtc((long)l);
            return dt;
        }

        public static HRESULT CreateArrayNode(INativeContactProperties contact, string arrayName, bool appendNode, out string node)
        {
            node = null;
            Verify.IsNotNull(contact, "contact");

            HRESULT hr;
            var sb = new StringBuilder((int)Win32Value.MAX_PATH);
            uint convertedAppend = appendNode ? Win32Value.TRUE : Win32Value.FALSE;
            uint cch;

            hr = contact.CreateArrayNode(arrayName, ContactValue.CGD_DEFAULT, convertedAppend, sb, (uint)sb.Capacity, out cch);
            
            // If we didn't have enough space for the node the first time through, try the bigger size.
            if (Win32Error.ERROR_INSUFFICIENT_BUFFER == hr)
            {
                sb.EnsureCapacity((int)cch);
                hr = contact.CreateArrayNode(arrayName, ContactValue.CGD_DEFAULT, convertedAppend, sb, (uint)sb.Capacity, out cch);

                // If this failed a second time, it shouldn't be because of an insufficient buffer.
                Assert.Implies(hr.Failed(), Win32Error.ERROR_INSUFFICIENT_BUFFER != hr);
            }

            if (hr.Succeeded())
            {
                node = sb.ToString();
            }

            return hr;
        }

        public static HRESULT DeleteArrayNode(INativeContactProperties contact, string nodeName)
        {
            Verify.IsNotNull(contact, "contact");
            Verify.IsNotNull(nodeName, "nodeName");

            // COM APIs don't check for this.  DeleteProperty should be used in this case.
            if (!nodeName.EndsWith("]", StringComparison.Ordinal))
            {
                return Win32Error.ERROR_INVALID_DATATYPE;
            }

            return contact.DeleteArrayNode(nodeName, ContactValue.CGD_DEFAULT);
        }

        public static HRESULT DeleteLabels(INativeContactProperties contact, string nodeName)
        {
            Verify.IsNotNull(contact, "contact");

            return contact.DeleteLabels(nodeName, ContactValue.CGD_DEFAULT);
        }

        public static HRESULT DeleteProperty(INativeContactProperties contact, string propertyName)
        {
            Verify.IsNotNull(contact, "contact");
            Verify.IsNotNull(propertyName, "propertyName");

            // COM APIs don't check for this.  DeleteArrayNode should be used in this case.
            if (propertyName.EndsWith("]", StringComparison.Ordinal))
            {
                return Win32Error.ERROR_INVALID_DATATYPE;
            }

            return contact.DeleteProperty(propertyName, ContactValue.CGD_DEFAULT);
        }


        // There's a bug in Windows Contacts that simple extension array nodes return S_OK
        // instead of S_FALSE.  This function happens to behave correctly anyways.
        public static bool DoesPropertyExist(INativeContactProperties contact, string propertyName)
        {
            Verify.IsNotNull(contact, "contact");

            if (string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            string dummy;
            HRESULT hr = GetString(contact, propertyName, false, out dummy);
            if (HRESULT.S_FALSE == hr)
            {
                // S_FALSE usually implies a deleted property,
                // but if it's an array node then it's present.
                return propertyName.EndsWith("]", StringComparison.Ordinal);
            }
            if (Win32Error.ERROR_PATH_NOT_FOUND == hr)
            {
                return false;
            }
            // Other errors are unexpected.
            hr.ThrowIfFailed("Error querying the property");
            return true;
        }

        public static HRESULT GetBinary(INativeContactProperties contact, string propertyName, bool ignoreDeletes, out string binaryType, out Stream binary)
        {
            binaryType = null;
            binary = null;
            Verify.IsNotNull(contact, "contact");
            Verify.IsNotNull(propertyName, "propertyName");

            HRESULT hr;
            var sb = new StringBuilder((int)Win32Value.MAX_PATH);
            IStream stm = null;

            try
            {
                uint cch;
                hr = contact.GetBinary(propertyName, ContactValue.CGD_DEFAULT, sb, (uint)sb.Capacity, out cch, out stm);
                if (ignoreDeletes && HRESULT.S_FALSE == hr)
                {
                    hr = Win32Error.ERROR_PATH_NOT_FOUND;
                }
                // If we didn't have enough space for the binaryType the first time through, try the bigger size.
                if (Win32Error.ERROR_INSUFFICIENT_BUFFER == hr)
                {
                    Assert.IsNull(stm);
                    sb.EnsureCapacity((int)cch);
                    hr = contact.GetBinary(propertyName, ContactValue.CGD_DEFAULT, sb, (uint)sb.Capacity, out cch, out stm);
                    // GetBinary shouldn't return ERROR_INSUFFICIENT_BUFFER if it's going to subsequently return S_FALSE.
                    Assert.AreNotEqual(HRESULT.S_FALSE, hr);
                    // If this failed a second time, it shouldn't be because of an insufficient buffer.
                    Assert.Implies(hr.Failed(), Win32Error.ERROR_INSUFFICIENT_BUFFER != hr);
                }

                if (HRESULT.S_OK == hr)
                {
                    binary = new ComStream(ref stm);
                    binaryType = sb.ToString();
                }
            }
            finally
            {
                Utility.SafeRelease(ref stm);
            }

            return hr;
        }

        public static HRESULT GetDate(INativeContactProperties contact, string propertyName, bool ignoreDeletes, out DateTime value)
        {
            value = default(DateTime);
            Verify.IsNotNull(contact, "contact");
            Verify.IsNotNull(propertyName, "propertyName");

            FILETIME ft;
            HRESULT hr = contact.GetDate(propertyName, ContactValue.CGD_DEFAULT, out ft);
            // If the caller doesn't care about deleted properties, convert the error code.
            if (ignoreDeletes && HRESULT.S_FALSE == hr)
            {
                hr = Win32Error.ERROR_PATH_NOT_FOUND;
            }

            if (HRESULT.S_OK == hr)
            {
                value = DateTimeFromFILETIME(ft);
            }

            return hr;
        }

        public static HRESULT GetLabeledNode(INativeContactProperties contact, string collection, string[] labels, out string labeledNode)
        {
            labeledNode = null;
            Verify.IsNotNull(contact, "contact");
            Verify.IsNotNull(collection, "collection");

            if (null == labels)
            {
                labels = new string[0];
            }

            // Make a copy of the label set.
            // We're going to take two passes while trying to find the labeled value.
            // One has the Preferred label, the second doesn't.
            var preferredLabels = new string[labels.Length + 1];
            labels.CopyTo(preferredLabels, 0);
            preferredLabels[labels.Length] = PropertyLabels.Preferred;

            HRESULT hr;
            IContactPropertyCollection propertyCollection = null;

            try
            {
                hr = GetPropertyCollection(contact, collection, preferredLabels, false, out propertyCollection);
                if (hr.Succeeded())
                {
                    // If a node satisfies this constraint, use it.
                    hr = propertyCollection.Next();
                    if (HRESULT.S_FALSE == hr)
                    {
                        // Otherwise, try it again without the extra "Preferred" label.
                        Utility.SafeRelease(ref propertyCollection);
                        hr = GetPropertyCollection(contact, collection, labels, false, out propertyCollection);
                        if (hr.Succeeded())
                        {
                            // Does an array node exist with these labels?
                            hr = propertyCollection.Next();
                            // There's nothing left to fall back on.  S_FALSE implies this property doesn't exist.
                            if (HRESULT.S_FALSE == hr)
                            {
                                hr = Win32Error.ERROR_PATH_NOT_FOUND;
                            }
                        }
                    }
                }

                if (hr.Succeeded())
                {
                    hr = GetPropertyName(propertyCollection, out labeledNode);
                }
            }
            finally
            {
                Utility.SafeRelease(ref propertyCollection);
            }

            return hr;
        }

        public static HRESULT GetLabels(INativeContactProperties contact, string arrayNode, out List<string> labels)
        {
            HRESULT hr;
            labels = null;

            Verify.IsNotNull(contact, "contact");

            using (var marshalable = new MarshalableDoubleNullString(Win32Value.MAX_PATH))
            {
                uint cch;
                hr = contact.GetLabels(arrayNode, ContactValue.CGD_DEFAULT, marshalable.MarshaledString, marshalable.Capacity, out cch);
                // If we didn't have enough space for the node the first time through, try the bigger size.
                if (Win32Error.ERROR_INSUFFICIENT_BUFFER == hr)
                {
                    // Reallocate to the size returned by the last GetLabels call.
                    marshalable.Realloc(cch);

                    hr = contact.GetLabels(arrayNode, ContactValue.CGD_DEFAULT, marshalable.MarshaledString, marshalable.Capacity, out cch);
                    // If this failed a second time, it shouldn't be because of an insufficient buffer.
                    Assert.Implies(hr.Failed(), Win32Error.ERROR_INSUFFICIENT_BUFFER != hr);
                }

                if (hr.Succeeded())
                {
                    labels = marshalable.ParsedStrings;
                }
            }

            return hr;
        }

        public static HRESULT GetPropertyCollection(INativeContactProperties contact, string collection, string[] labels, bool anyLabelMatches, out IContactPropertyCollection propertyCollection)
        {
            Verify.IsNotNull(contact, "contact");

            uint fAnyLabelMatches = anyLabelMatches ? Win32Value.TRUE : Win32Value.FALSE;

            using (var mlc = new MarshalableLabelCollection(labels))
            {
                return contact.GetPropertyCollection(out propertyCollection, ContactValue.CGD_DEFAULT, collection, mlc.Count, mlc.MarshaledLabels, fAnyLabelMatches);
            }
        }

        public static HRESULT GetPropertyName(IContactPropertyCollection propertyCollection, out string name)
        {
            name = null;
            Verify.IsNotNull(propertyCollection, "propertyCollection");

            var sb = new StringBuilder((int)Win32Value.MAX_PATH);
            uint cch;
            HRESULT hr = propertyCollection.GetPropertyName(sb, (uint)sb.Capacity, out cch);
            // If we didn't have enough space for the node the first time through, try the bigger size.
            if (Win32Error.ERROR_INSUFFICIENT_BUFFER == hr)
            {
                sb.EnsureCapacity((int)cch);
                hr = propertyCollection.GetPropertyName(sb, (uint)sb.Capacity, out cch);

                // If this failed a second time, it shouldn't be because of an insufficient buffer.
                Assert.Implies(hr.Failed(), Win32Error.ERROR_INSUFFICIENT_BUFFER != hr);
            }

            if (hr.Succeeded())
            {
                name = sb.ToString();
            }

            return hr;
        }

        public static HRESULT GetString(INativeContactProperties contact, string propertyName, bool ignoreDeletes, out string value)
        {
            value = null;
            Verify.IsNotNull(contact, "contact");
            Verify.IsNotNull(propertyName, "propertyName");

            uint cch;
            var sb = new StringBuilder((int)Win32Value.MAX_PATH);
            HRESULT hr = contact.GetString(propertyName, ContactValue.CGD_DEFAULT, sb, (uint)sb.Capacity, out cch);
            // If the caller doesn't care about deleted properties, convert the error code.
            if (ignoreDeletes && HRESULT.S_FALSE == hr)
            {
                hr = Win32Error.ERROR_PATH_NOT_FOUND;
            }
            // If we didn't have enough space for the value the first time through, try the bigger size.
            if (Win32Error.ERROR_INSUFFICIENT_BUFFER == hr)
            {
                sb.EnsureCapacity((int)cch);
                hr = contact.GetString(propertyName, ContactValue.CGD_DEFAULT, sb, (uint)sb.Capacity, out cch);

                // If this failed a second time, it shouldn't be because of an insufficient buffer.
                Assert.Implies(hr.Failed(), Win32Error.ERROR_INSUFFICIENT_BUFFER != hr);
            }

            if (HRESULT.S_OK == hr)
            {
                value = sb.ToString();
            }

            return hr;
        }

        /// <summary>
        /// Tokenizes an unmanaged WCHAR array of multiple embedded strings into a List{string}
        /// </summary>
        /// <param name="doubleNullString">
        /// An IntPtr that points to an unmanaged WCHAR[] containing multiple strings.
        /// Each string in the parameter is terminated by a null character.  The parameter
        /// itself is terminated by a pair of null characters.  If the parameter begins with
        /// a null character, it doesn't necessarily need a second terminating null.
        /// </param>
        /// <returns>
        /// A list of the embedded strings in the doubleNullString parameter.
        /// If there are no strings in the parameter, then an empty string is returned.
        /// </returns>
        /// <exception cref="System.ArgumentNullException" >
        /// doubleNullString must point at valid memory.
        /// </exception>
        public static List<string> ParseDoubleNullString(IntPtr doubleNullString)
        {
            if (IntPtr.Zero == doubleNullString)
            {
                throw new ArgumentNullException("doubleNullString");
            }

            var results = new List<string>();

            IntPtr currentPtr = doubleNullString;
            while (true)
            {
                string fragment = Marshal.PtrToStringUni(currentPtr);
                Assert.IsNotNull(fragment);

                // This might catch even when currentPtr == doubleNullString.
                // If the parameter is empty, then this function doesn't care about a second null.
                if (string.IsNullOrEmpty(fragment))
                {
                    break;
                }

                results.Add(fragment);
                currentPtr = (IntPtr)((int)currentPtr + (fragment.Length + 1) * Win32Value.sizeof_WCHAR);
            }

            return results;
        }

        /// <summary>
        /// Utility to set a binary property on an INativeContactProperties.
        /// </summary>
        /// <param name="contact">The INativeContactProperties to set the value on.</param>
        /// <param name="propertyName">The property to set.</param>
        /// <param name="binary">The value to set to the property.</param>
        /// <param name="binaryType">The mime-type of the value being applied.</param>
        /// <returns>HRESULT.</returns>
        /// <remarks>
        /// This is a thin wrapper over the COM INativeContactProperties::SetBinary to make it more easily consumable
        /// in .Net.  Behavior and returned error codes should be similar to the native version.
        /// </remarks>
        public static HRESULT SetBinary(INativeContactProperties contact, string propertyName, string binaryType, Stream binary)
        {
            Verify.IsNotNull(contact, "contact");
            Verify.IsNotNull(propertyName, "propertyName");

            using (var mstream = new ManagedIStream(binary))
            {
                mstream.Seek(0, (int)SeekOrigin.Begin, IntPtr.Zero);
                return contact.SetBinary(propertyName, ContactValue.CGD_DEFAULT, binaryType, mstream);
            }
        }

        /// <summary>
        /// Utility to set a date property on an INativeContactProperties.
        /// </summary>
        /// <param name="contact">The INativeContactProperties to set the value on.</param>
        /// <param name="propertyName">The property to set.</param>
        /// <param name="value">The date value to set to the property.</param>
        /// <returns>HRESULT.</returns>
        /// <remarks>
        /// This is a thin wrapper over the COM INativeContactProperties::SetDate to make it more easily consumable
        /// in .Net.  Behavior and returned error codes should be similar to the native version.
        /// </remarks>
        public static HRESULT SetDate(INativeContactProperties contact, string propertyName, DateTime value)
        {
            Verify.IsNotNull(contact, "contact");
            Verify.IsNotNull(propertyName, "propertyName");

            // If the caller hasn't explicitly set the kind then assume it's UTC
            // so it will be written as read to the Contact.  
            if (value.Kind != DateTimeKind.Local)
            {
                value = new DateTime(value.Ticks, DateTimeKind.Utc);
            }

            long longFiletime = value.ToFileTime();

            var ft = new FILETIME
            {
                dwLowDateTime = (Int32)longFiletime,
                dwHighDateTime = (Int32)(longFiletime >> 32)
            };

            return contact.SetDate(propertyName, ContactValue.CGD_DEFAULT, ft);
        }

        /// <summary>
        /// Utility to augment the label set on a preexisting array node in an INativeContactProperties.
        /// </summary>
        /// <param name="contact">The INativeContactProperties where the labels are to be set.</param>
        /// <param name="arrayNode">The array node to apply the labels to.</param>
        /// <param name="labels">The labels to add to the array node.</param>
        /// <returns>HRESULT.</returns>
        /// <remarks>
        /// This is a thin wrapper over the COM INativeContactProperties::SetLabels to make it more easily consumable
        /// in .Net.  Behavior and returned error codes should be similar to the native version.
        /// </remarks>
        public static HRESULT SetLabels(INativeContactProperties contact, string arrayNode, ICollection<string> labels)
        {
            Verify.IsNotNull(contact, "contact");

            using (var marshalable = new MarshalableLabelCollection(labels))
            {
                return contact.SetLabels(arrayNode, ContactValue.CGD_DEFAULT, marshalable.Count, marshalable.MarshaledLabels);
            }
        }

        /// <summary>
        /// Utility to set a string property on an INativeContactProperties.
        /// </summary>
        /// <param name="contact">The INativeContactProperties to set the value on.</param>
        /// <param name="propertyName">The property to set.</param>
        /// <param name="value">The value to set to the property.</param>
        /// <returns>HRESULT.</returns>
        /// <remarks>
        /// This is a thin wrapper over the COM INativeContactProperties::SetString to make it more easily consumable
        /// in .Net.  Behavior and returned error codes should be similar to the native version.
        /// </remarks>
        public static HRESULT SetString(INativeContactProperties contact, string propertyName, string value)
        {
            Verify.IsNotNull(contact, "contact");
            Verify.IsNotNull(propertyName, "propertyName");

            return contact.SetString(propertyName, ContactValue.CGD_DEFAULT, value);
        }
    }
}

#endif