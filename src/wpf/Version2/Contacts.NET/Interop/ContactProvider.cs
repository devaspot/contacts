/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.ContactsBridge.Interop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;

    // Resolving ambiguity between System.Runtime.InteropServices.FILETIME
    using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;
	using Standard.Interop;

    #region Other COM Interfaces

    /// <exclude/>
    [
        Flags
    ]
    public enum STGM : uint
    {
        /// <exclude/>
        DIRECT = 0x00000000,
        /// <exclude/>
        TRANSACTED = 0x00010000,
        /// <exclude/>
        SIMPLE = 0x08000000,
        /// <exclude/>
        READ = 0x00000000,
        /// <exclude/>
        WRITE = 0x00000001,
        /// <exclude/>
        READWRITE = 0x00000002,
        /// <exclude/>
        SHARE_DENY_NONE = 0x00000040,
        /// <exclude/>
        SHARE_DENY_READ = 0x00000030,
        /// <exclude/>
        SHARE_DENY_WRITE = 0x00000020,
        /// <exclude/>
        SHARE_EXCLUSIVE = 0x00000010,
        /// <exclude/>
        PRIORITY = 0x00040000,
        /// <exclude/>
        DELETEONRELEASE = 0x04000000,
        /// <exclude/>
        NOSCRATCH = 0x00100000,
        /// <exclude/>
        CREATE = 0x00001000,
        /// <exclude/>
        CONVERT = 0x00020000,
        /// <exclude/>
        FAILIFTHERE = 0x00000000,
        /// <exclude/>
        NOSNAPSHOT = 0x00200000,
        /// <exclude/>
        DIRECT_SWMR = 0x00400000,
    }

    [
        StructLayout(LayoutKind.Sequential, Pack = 4)
    ]
    public struct CONNECTDATA
    {
        [MarshalAs(UnmanagedType.IUnknown)]
        public object pUnk;
        public uint dwCookie;
    }

    /// <exclude/>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.IPersist),
    ]
    public interface IPersist
    {
        /// <exclude/>
        void GetClassID([Out] out Guid pClassID);
    }

    /// <exclude/>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.IPersistFile),
    ]
    public interface IPersistFile : IPersist
    {
        /// <exclude/>
        // Need to redeclare base interface's methods to properly generate the vtable.
        new void GetClassID([Out] out Guid pClassID);

        /// <exclude/>
        [PreserveSig]
        HRESULT IsDirty();

        /// <exclude/>
        [PreserveSig]
        HRESULT Load([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
                  [In] STGM dwMode);

        /// <exclude/>
        void Save([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
                  [In, MarshalAs(UnmanagedType.Bool)] bool fRemember);

        /// <exclude/>
        void SaveCompleted([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

        /// <exclude/>
        [Obsolete("The LPWSTR is allocated via IMalloc.  "
            + "Be careful if you decide to use this function because you're responsible for freeing the memory.",
            false)]
        void GetCurFile([Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }

    /// <exclude/>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.IPersistStream)]
    public interface IPersistStream : IPersist
    {
        /// <exclude/>
        // Need to redeclare base interface's methods to properly generate the vtable.
        new void GetClassID([Out] out Guid pClassID);

        /// <exclude/>
        [PreserveSig]
        HRESULT IsDirty();

        /// <exclude/>
        [PreserveSig]
        HRESULT Load([In, MarshalAs(UnmanagedType.Interface)] IStream pstm);

        /// <exclude/>
        void Save([In, MarshalAs(UnmanagedType.Interface)] IStream pstm, [In, MarshalAs(UnmanagedType.Bool)] bool fClearDirty);

        /// <exclude/>
        void GetSizeMax([Out] out ulong pcbSize);
    }

    /// <exclude/>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.IPersistStreamInit),
    ]
    public interface IPersistStreamInit : IPersist
    {
        /// <exclude/>
        // Need to redeclare base interface's methods to properly generate the vtable.
        new void GetClassID([Out] out Guid pClassID);

        /// <exclude/>
        [PreserveSig]
        HRESULT IsDirty();

        /// <exclude/>
        [PreserveSig]
        HRESULT Load([In, MarshalAs(UnmanagedType.Interface)] IStream pstm);

        /// <exclude/>
        void Save([In, MarshalAs(UnmanagedType.Interface)] IStream pstm, [In, MarshalAs(UnmanagedType.Bool)] bool fClearDirty);

        /// <exclude/>
        void GetSizeMax([Out] out ulong pcbSize);

        /// <exclude/>
        void InitNew();
    }


    [
        ComImport,
        Guid(IIDGuid.IPropertyNotifySink),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
    ]
    public interface IPropertyNotifySink
    {
        /// <exclude/>
        void OnChanged([In] int dispID);

        /// <exclude/>
        void OnRequestEdit([In] int dispID);
    }

    #endregion

    #region IContact Interfaces

    /// <summary>
    /// This interface is used to interact with a contact with a ContactID string.
    /// </summary>
    /// <remarks>
    /// Because this interface is very close to COM, returning error codes are
    /// generally preferred to throwing exceptions.
    /// </remarks>
    // From what I can tell, the CoClassAttribute on this interface allows the caller to cast an
    // IContactManager reference to a ContactManagerRcw class view.  Without it, an exception is thrown.
    [
        ComImport,
        CoClass(typeof(ContactManagerRcw)),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.IContactManager)
    ]
    public interface IContactManager
    {
        /// <summary>
        /// Initialize the contact manager with the unique application name and
        /// application version being used to manipulate contacts.
        /// </summary>
        /// <remarks>
        /// Initialize must be called before other IContactManager methods.</remarks>
        /// <param name="pszAppName">The application name</param>
        /// <param name="pszAppVersion">The application version</param>
        [PreserveSig]
        HRESULT Initialize([In, MarshalAs(UnmanagedType.LPWStr)] string pszAppName,
                           [In, MarshalAs(UnmanagedType.LPWStr)] string pszAppVersion);

        /// <summary>
        /// Load an IContact object with the data from the contact referenced by
        /// the machine local ContactID pszContactID.
        /// </summary>
        /// <returns>
        /// Returns S_OK on success, HRESULT_FROM_WIN32(ERROR_NO_MATCH) if the
        /// specified contact ID couldn't be found.
        /// </returns>
        /// <param name="pszContactID">The ContactID of the IContact to load</param>
        /// <param name="ppContact">The loaded contact object</param>
        [PreserveSig]
        HRESULT Load([In, MarshalAs(UnmanagedType.LPWStr)] string pszContactID,
                     [Out, MarshalAs(UnmanagedType.Interface)] out IContact ppContact);

        /// <summary>
        /// Make future resolutions against oldContactID resolve to contact newContactID.
        /// </summary>
        /// <remarks>
        /// This function is called when the caller has merged the properties of
        /// oldContactID into the contact referenced by newContactID, and before
        /// deleting contact oldContactID.
        /// </remarks>
        /// <returns>Returns S_OK on success, non-zero on failure.</returns>
        /// <param name="pszNewContactID">
        /// The ContactID of the new contact representing both the old and new contact.
        /// </param>
        /// <param name="pszOldContactID">
        /// The ContactID to that was deleted when its data was merged into newContactID.
        /// </param>
        [PreserveSig]
        HRESULT MergeContactIDs([In, MarshalAs(UnmanagedType.LPWStr)] string pszNewContactID,
                                [In, MarshalAs(UnmanagedType.LPWStr)] string pszOldContactID);

        /// <summary>
        /// Gets the local user account's concept of "Me".
        /// </summary>
        /// <param name="ppMeContact">The Me contact.</param>
        /// <returns>Returns S_OK on success.</returns>
        [PreserveSig]
        HRESULT GetMeContact([Out, MarshalAs(UnmanagedType.Interface)] out IContact ppMeContact);

        /// <summary>
        /// Sets the local user account's concept of "Me".
        /// </summary>
        /// <param name="pMeContact">The Me contact.</param>
        /// <returns>Returns S_OK on success.</returns>
        [PreserveSig]
        HRESULT SetMeContact([In, MarshalAs(UnmanagedType.Interface)] IContact pMeContact);

        /// <summary>
        /// Get an enumerator for all known contacts.
        /// </summary>
        /// <param name="ppContactCollection">An enumerator over the user's contacts.</param>
        [PreserveSig]
        HRESULT GetContactCollection([Out, MarshalAs(UnmanagedType.Interface)] out IContactCollection ppContactCollection);
    }

    /// <summary>
    /// Enumerates the contacts known by the IContactManager.
    /// </summary>
    /// <remarks>
    /// Adding or removing contacts through other means while this is performing
    /// an enumeration results in undefined behavior.  Currently this interface
    /// does not support deletion of contacts during an enumeration.  Otherwise
    /// modifying Contact's properties should not affect this.<para/>
    /// Because this interface is very close to COM, returning error codes are
    /// generally preferred to throwing exceptions.
    /// </remarks>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.IContactCollection),
    ]
    public interface IContactCollection
    {
        /// <summary>
        /// Reset enumeration of contacts.  The enumerator resides before the first contact.
        /// </summary>
        /// <remarks>
        /// A call to GetCurrent immediately after Reset is undefined.  To get
        /// the first Contact, first call Next() to ensure that there is one.
        /// </remarks>
        void Reset();

        /// <summary>
        /// Move to the next contact in the enumeration.
        /// </summary>
        /// <remarks>
        /// After S_FALSE is returned, further calls to GetCurrent will fail.
        /// Repeated calls to Next after S_FALSE is returned without calling
        /// Reset have undefined return values.
        /// Calling GetCurrent after a Reset without a call to Next between will
        /// fail.
        /// </remarks>
        /// <returns>
        /// S_OK if GetCurrent will yield a Contact.
        /// S_FALSE if there are no more contacts.
        /// Other non-zero value if an error occurred.
        /// </returns>
        [PreserveSig]
        HRESULT Next();

        /// <summary>
        /// Retrieve the current contact in the enumeration.
        /// </summary>
        /// <param name="ppContact">The current contact in the enumeration.</param>
        void GetCurrent([Out, MarshalAs(UnmanagedType.Interface)] out IContact ppContact);
    }

    /// <summary>
    /// This interface is used to get, set, create and remove properties on an IContact.
    /// </summary>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.IContactProperties),
    ]
    public interface IContactProperties
    {
        /// <summary>
        /// Retrieve the string value at pszPropertyName into a string buffer.
        /// </summary>
        /// <remarks>
        /// To retrieve a single level property, set pszPropertyName to the property name.
        /// To retrieve a property from a multi value property, set pszPropertyName to the form:
        /// "toplevel/secondlevel[4]/thirdlevel".<para/>
        /// Note: the first element of a set is index 1.  GetString with [0] is invalid
        /// </remarks>
        /// <param name="pszPropertyName">property to retrieve</param>
        /// <param name="dwFlags">Must be CGD_DEFAULT (0)</param>
        /// <param name="pszValue">StringBuilder where the value is stored</param>
        /// <param name="cchValue">Capacity of the StringBuilder object.</param>
        /// <param name="pdwcchPropertyValueRequired">
        /// On failure due to insufficient capacity, this contains the required capacity for pszValue
        /// </param>
        /// <returns>
        /// <list type="Return Values">
        /// <item>S_OK - pszValue contains the NULL terminated value</item>
        /// <item>S_FALSE - No data for this value.  Either the property has been present in the past
        /// but its value has been removed, or the property is a container of other properties
        /// (toplevel/secondlevel[3]).<para/>
        /// The buffer at pszValue has been zero'ed.</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_PATH_NOT_FOUND) - no data found for this property name.</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER) - pszValue was not large enough to
        /// store the value.  The required buffer size is stored pdwcchPropertyValueRequired.</item>
        /// <item>Other FAILED HRESULTs.</item>
        /// </list>
        /// </returns>
        [PreserveSig]
        HRESULT GetString([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
                          [In] uint dwFlags,
                          [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszValue,
                          [In] uint cchValue,
                          [Out] out uint pdwcchPropertyValueRequired);

        /// <summary>
        /// Retrieve the date and time value at pszPropertyName into a caller's FILETIME structure.
        /// </summary>
        /// <remarks>All times are stored and returned as UTC time.</remarks>
        /// <param name="pszPropertyName">Property to retrieve.</param>
        /// <param name="dwFlags">Reserved.  Must be GCD_DEFAULT.</param>
        /// <param name="pftDateTime">Reference to a filetime structure where the value will be stored.</param>
        /// <returns>
        /// <list type="Return Values">
        /// <item>S_OK - pftDateTime contains a valid FILETIME.</item>
        /// <item>S_FALSE - No data for this value.  This property has been present in the past,
        /// but its value has been removed.  The FILETIME has been zero'ed.</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_PATH_NOT_FOUND) - no data found for this property name.</item>
        /// <item>Other FAILED HRESULTs.</item>
        /// </list>
        /// </returns>
        [PreserveSig]
        HRESULT GetDate([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
                        [In] uint dwFlags,
                        [Out] out FILETIME pftDateTime);

        /// <summary>
        /// Retrieve the binary data at pszPropertyName via an IStream.
        /// </summary>
        /// <remarks>
        /// To retrieve a single level property, set pszPropertyName to the property name.
        /// To retrieve a property from a multi value property, set pszPropertyName to the form:
        /// "toplevel/secondlevel[4]/thirdlevel".
        /// GetBinary for properties that have been deleted return S_FALSE and a NULL IStream reference.
        /// NOTE: GetBinary for properties that are not of binary type may return incorrect data in the IStream</remarks>
        /// <param name="pszPropertyName">property to retrieve.</param>
        /// <param name="dwFlags">Reserved.  Must be CGD_DEFAULT.</param>
        /// <param name="pszContentType">User allocated buffer to store the mime content type in.</param>
        /// <param name="cchContentType">Allocated buffer size in characters.</param>
        /// <param name="pdwcchContentTypeRequired">on failure, contains the required size for pszContentType.</param>
        /// <param name="ppStream">on SUCCESS, contains a new IStream reference.  Use this to retrieve the binary data.</param>
        /// <returns>
        /// <list type="Return Values">
        /// <item>S_OK - ppStream contains an IStream*.  Caller must release the refrence.</item>
        /// <item>
        /// S_FALSE - The binary data has been deleted.
        /// ppStream does not contain a reference.  pszContentType has been zeroed.
        /// </item>
        /// <item>HRESULT_FROM_WIN32(ERROR_PATH_NOT_FOUND) - no data found for this property name.</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_INVALID_DATATYPE) - unable to get this value for this
        /// property due to schema</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER) - pszValue was not large enough to
        /// store the value.  The required buffer size is stored in pdwcchContentTypeRequired.</item>
        /// <item>Other FAILED HRESULTs</item>
        /// </list>
        /// </returns>
        [PreserveSig]
        HRESULT GetBinary([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
                          [In] uint dwFlags,
                          [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszContentType,
                          [In] uint cchContentType,
                          [Out] out uint pdwcchContentTypeRequired,
                          [Out, MarshalAs(UnmanagedType.Interface)] out IStream ppStream);

        

        /// <summary>
        /// Retrieve the labels for a named array node.
        /// </summary>
        /// <remarks>
        /// Warning: pszLabels is a list of strings concatenated together,
        /// followed by an empty string.  When callers are parsing this, they must look for
        /// two adjacent null-terminating characters.
        /// This function may return labels in a different order than they were set in.
        /// </remarks>
        /// <param name="pszArrayElementName">Name of the property to retrieve the labels for.</param>
        /// <param name="dwFlags">Reserved.  Must be CGD_DEFAULT.</param>
        /// <param name="pszLabels">User allocated buffer to store the labels in.</param>
        /// <param name="cchLabels">Size reserved by the caller for pszLabels.</param>
        /// <param name="pdwcchLabelsRequired">on FAILURE, contains the required size for pszLabels.</param>
        /// <returns>
        /// <list type="Return Values">
        /// <item>S_OK - pszLabels contains the set of labels.</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_PATH_NOT_FOUND) - no data found for this property name.</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_INVALID_DATATYPE) - unable to get this value for this
        /// property due to schema</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER) - pszLabels was not large enough to
        /// store the value.  The required buffer size is stored in pdwcchLabelsRequired.</item>
        /// <item>Other FAILED HRESULTs</item>
        /// </list>
        /// </returns>
        [PreserveSig]
        HRESULT GetLabels([In, MarshalAs(UnmanagedType.LPWStr)] string pszArrayElementName,
                          [In] uint dwFlags,
                          [In, Out, MarshalAs(UnmanagedType.LPWStr)] IntPtr pszLabels,
                          [In] uint cchLabels,
                          [Out] out uint pdwcchLabelsRequired);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszPropertyName"></param>
        /// <param name="dwFlags"></param>
        /// <param name="pszValue"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT SetString([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
                          [In] uint dwFlags,
                          [In, MarshalAs(UnmanagedType.LPWStr)] string pszValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszPropertyName"></param>
        /// <param name="dwFlags"></param>
        /// <param name="ftDateTime"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT SetDate([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
                        [In] uint dwFlags,
                        [In] FILETIME ftDateTime);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszPropertyName"></param>
        /// <param name="dwFlags"></param>
        /// <param name="pszContentType"></param>
        /// <param name="pStream"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT SetBinary([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
                          uint dwFlags,
                          [In, MarshalAs(UnmanagedType.LPWStr)] string pszContentType,
                          [In, MarshalAs(UnmanagedType.Interface)] IStream pStream);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszArrayElementName"></param>
        /// <param name="dwFlags"></param>
        /// <param name="dwLabelCount"></param>
        /// <param name="ppszLabels"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT SetLabels([In, MarshalAs(UnmanagedType.LPWStr)] string pszArrayElementName,
                          [In] uint dwFlags,
                          [In] uint dwLabelCount,
                          [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2, ArraySubType=UnmanagedType.LPWStr)] IntPtr ppszLabels);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszArrayName"></param>
        /// <param name="dwFlags"></param>
        /// <param name="fAppend">
        /// A Boolean value exposed as an UInt32.  Acceptable values are 1 (TRUE) and 0 (FALSE).
        /// Experimentally, the Vista API have odd behavior when this is marshalled as a System.Boolean,
        /// as it only accepts those two values.
        /// </param>
        /// <param name="pszNewArrayElementName"></param>
        /// <param name="cchNewArrayElementName"></param>
        /// <param name="pdwcchNewArrayElementNameRequired"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT CreateArrayNode([In, MarshalAs(UnmanagedType.LPWStr)] string pszArrayName,
                                [In] uint dwFlags,
                                [In] uint fAppend,
                                [In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszNewArrayElementName,
                                [In] uint cchNewArrayElementName,
                                [Out] out uint pdwcchNewArrayElementNameRequired);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszPropertyName"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT DeleteProperty([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
                               [In] uint dwFlags);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszArrayElementName"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT DeleteArrayNode([In, MarshalAs(UnmanagedType.LPWStr)] string pszArrayElementName,
                                [In] uint dwFlags);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszArrayElementName"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT DeleteLabels([In, MarshalAs(UnmanagedType.LPWStr)] string pszArrayElementName,
                             [In] uint dwFlags);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ppPropertyCollection"></param>
        /// <param name="dwFlags"></param>
        /// <param name="pszMultiValueName"></param>
        /// <param name="dwLabelCount"></param>
        /// <param name="ppszLabels"></param>
        /// <param name="fAnyLabelMatches"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT GetPropertyCollection([Out, MarshalAs(UnmanagedType.Interface)] out IContactPropertyCollection ppPropertyCollection,
                                      [In] uint dwFlags,
                                      [In, MarshalAs(UnmanagedType.LPWStr)] string pszMultiValueName,
                                      [In] uint dwLabelCount,
                                      [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=3)] IntPtr ppszLabels,
                                      [In] uint fAnyLabelMatches);
    }

    /// <summary>
    /// An enumerator for properties exposed by an IContactProperties object.
    /// </summary>
    /// <remarks>
    /// For each property, the name, type, version, and modification-date can be queried.
    /// Changing the IContactProperties object while enumerating properties with this
    /// interface results in undefined behavior.
    /// </remarks>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.IContactPropertyCollection),
    ]
    public interface IContactPropertyCollection
    {
        /// <summary>
        /// Reset the property enumerator.
        /// </summary>
        void Reset();

        /// <summary>
        /// Move the enumerator to the next property.
        /// </summary>
        /// <remarks>
        /// After S_FALSE is first returned, subsequent calls to this' GetProperty*
        /// functions will return FAILURE.
        /// </remarks>
        /// <returns>
        /// <list type="Return Values">
        /// <item>S_OK - moved to the next property.</item>
        /// <item>S_FALSE - at the end of the property enumeration.</item>
        /// <item>Other FAILED HRESULTs.</item>
        /// </list>
        /// </returns>
        [PreserveSig]
        HRESULT Next();

        /// <summary>
        /// Retrieve the propertyName for the current property in the enumeration.
        /// </summary>
        /// <param name="pszPropertyName">On SUCCESS, contains the name to use for calling Get*
        /// on IContactProperties.  E.g. "toplevel", or "toplevel/secondlevel[4]/thirdlevel"</param>
        /// <param name="cchPropertyName">Size of caller allocated pszPropertyName buffer, in characters.</param>
        /// <param name="pdwcchPropertyNameRequired">On FAILURE, contains the required size of pszPropertyName.</param>
        /// <returns>
        /// <list type="Return Values">
        /// <item>S_OK.</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER) - pszProperty was not large enough to
        /// store the property name.  The required buffer size is stored in pdwcchPropertyNameRequired.</item>
        /// <item>Other FAILED HRESULTs.</item>
        /// </list>
        /// </returns>
        [PreserveSig]
        HRESULT GetPropertyName([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszPropertyName,
                                [In] uint cchPropertyName,
                                [Out] out uint pdwcchPropertyNameRequired);

        /// <summary>
        /// Retrieve the propertyType for the current property in the enumeration.
        /// </summary>
        /// <param name="pdwType">Type of property stored at pszPropertyName.</param>
        /// <returns>
        /// <list type="Return Values">
        /// <item>S_OK.</item>
        /// <item>Other FAILED HRESULTs.</item>
        /// </list>
        /// </returns>
        [PreserveSig]
        HRESULT GetPropertyType([Out] out uint pdwType);

        /// <summary>
        /// Retrieve the version number for the current property in the enumeration.
        /// </summary>
        /// <param name="pdwVersion">Version number of property stored at pszPropertyName.</param>
        /// <returns>
        /// <list type="Return Values">
        /// <item>S_OK.</item>
        /// <item>Other FAILED HRESULTs.</item>
        /// </list>
        /// </returns>
        [PreserveSig]
        HRESULT GetPropertyVersion([Out] out uint pdwVersion);

        /// <summary>
        /// Retrieve the last modifictation date for the current property in the enumeration.
        /// </summary>
        /// <remarks>
        /// If this property was never modified, the contact's creation date is returned.
        /// </remarks>
        /// <param name="pftModificationDate">The last modified date as a UTC FILETIME.</param>
        /// <returns>
        /// <list type="Return Values">
        /// <item>S_OK.</item>
        /// <item>Other FAILED HRESULTs.</item>
        /// </list>
        /// </returns>
        [PreserveSig]
        HRESULT GetPropertyModificationDate([Out] out FILETIME pftModificationDate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszArrayElementID"></param>
        /// <param name="cchArrayElementID"></param>
        /// <param name="pdwcchArrayElementIDRequired"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT GetPropertyArrayElementID([In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArrayElementID,
                                          [In] uint cchArrayElementID,
                                          [Out] out uint pdwcchArrayElementIDRequired);
    }

    /// <summary>
    /// Handles reading and writing properties for a single contact.
    /// </summary>
    /// <remarks>
    /// Because this interface is very close to COM, returning error codes are
    /// generally preferred to throwing exceptions.
    /// 
    /// It's common for classes that implement this interface to also implement
    /// the following interfaces:
    /// IPersistFile:
    ///     Allows the contact to be loaded from a file.
    ///     Use this interface to load a contact to get full support from
    ///     IContact::CommitChanges change conflict detection.
    /// IPersistStream:
    /// IPersistStreamInit:
    ///     This interface allows the contact to be saved or loaded from a stream.
    ///     Use IPersistStreamInit::InitNew to create a new IContact
    ///     NOTE: loading a contact via IPersistStream will not have the locking
    ///     and conflict detection that comes with the usage of
    ///     IPersistFile::Load / IContact::CommitChanges
    /// IContactProperties:
    ///     Provides means for manipulation and querying of properties on this contact.
    /// </remarks>
    // From what I can tell, the CoClassAttribute on this interface allows the caller to cast an
    // IContact reference to a ContactRcw class view.  Without it, an exception is thrown.
    [
        ComImport,
        CoClass(typeof(ContactRcw)),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.IContact),
    ]
    public interface IContact
    {
        /// <summary>
        /// Retrieve the local machine unique ContactID.
        /// </summary>
        /// <param name="pszContactID"></param>
        /// <param name="cchContactID"></param>
        /// <param name="pdwcchContactIDRequired"></param>
        [PreserveSig]
        HRESULT GetContactID([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszContactID,
                             [In] uint cchContactID,
                             [Out] out uint pdwcchContactIDRequired);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszPath"></param>
        /// <param name="cchPath"></param>
        /// <param name="pdwcchPathRequired"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszPath,
                        [In] uint cchPath,
                        [Out] out uint pdwcchPathRequired);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dwCommitFlags"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT CommitChanges([In] uint dwCommitFlags);
    }

        // internal interface IContactProperties - Conflicts with Microsoft.Communications.Contacts.IContactProperties
	[
		ComImport,
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
		Guid(IIDGuid.IContactProperties),
	]
	public interface INativeContactProperties
    {
        /// <summary>
        /// Retrieve the string value at pszPropertyName into a string buffer.
        /// </summary>
        /// <remarks>
        /// To retrieve a single level property, set pszPropertyName to the property name.
        /// To retrieve a property from a multi value property, set pszPropertyName to the form:
        /// "toplevel/secondlevel[4]/thirdlevel".<para/>
        /// Note: the first element of a set is index 1.  GetString with [0] is invalid
        /// </remarks>
        /// <param name="pszPropertyName">property to retrieve</param>
        /// <param name="dwFlags">Must be CGD_DEFAULT (0)</param>
        /// <param name="pszValue">StringBuilder where the value is stored</param>
        /// <param name="cchValue">Capacity of the StringBuilder object.</param>
        /// <param name="pdwcchPropertyValueRequired">
        /// On failure due to insufficient capacity, this contains the required capacity for pszValue
        /// </param>
        /// <returns>
        /// <list type="Return Values">
        /// <item>S_OK - pszValue contains the NULL terminated value</item>
        /// <item>S_FALSE - No data for this value.  Either the property has been present in the past
        /// but its value has been removed, or the property is a container of other properties
        /// (toplevel/secondlevel[3]).<para/>
        /// The buffer at pszValue has been zero'ed.</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_PATH_NOT_FOUND) - no data found for this property name.</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER) - pszValue was not large enough to
        /// store the value.  The required buffer size is stored pdwcchPropertyValueRequired.</item>
        /// <item>Other FAILED HRESULTs.</item>
        /// </list>
        /// </returns>
        [PreserveSig]
        HRESULT GetString([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
                          [In] uint dwFlags,
                          [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszValue,
                          [In] uint cchValue,
                          [Out] out uint pdwcchPropertyValueRequired);

        /// <summary>
        /// Retrieve the date and time value at pszPropertyName into a caller's FILETIME structure.
        /// </summary>
        /// <remarks>All times are stored and returned as UTC time.</remarks>
        /// <param name="pszPropertyName">Property to retrieve.</param>
        /// <param name="dwFlags">Reserved.  Must be GCD_DEFAULT.</param>
        /// <param name="pftDateTime">Reference to a filetime structure where the value will be stored.</param>
        /// <returns>
        /// <list type="Return Values">
        /// <item>S_OK - pftDateTime contains a valid FILETIME.</item>
        /// <item>S_FALSE - No data for this value.  This property has been present in the past,
        /// but its value has been removed.  The FILETIME has been zero'ed.</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_PATH_NOT_FOUND) - no data found for this property name.</item>
        /// <item>Other FAILED HRESULTs.</item>
        /// </list>
        /// </returns>
        [PreserveSig]
        HRESULT GetDate([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
                        [In] uint dwFlags,
                        [Out] out FILETIME pftDateTime);

        /// <summary>
        /// Retrieve the binary data at pszPropertyName via an IStream.
        /// </summary>
        /// <remarks>
        /// To retrieve a single level property, set pszPropertyName to the property name.
        /// To retrieve a property from a multi value property, set pszPropertyName to the form:
        /// "toplevel/secondlevel[4]/thirdlevel".
        /// GetBinary for properties that have been deleted return S_FALSE and a NULL IStream reference.
        /// NOTE: GetBinary for properties that are not of binary type may return incorrect data in the IStream</remarks>
        /// <param name="pszPropertyName">property to retrieve.</param>
        /// <param name="dwFlags">Reserved.  Must be CGD_DEFAULT.</param>
        /// <param name="pszContentType">User allocated buffer to store the mime content type in.</param>
        /// <param name="cchContentType">Allocated buffer size in characters.</param>
        /// <param name="pdwcchContentTypeRequired">on failure, contains the required size for pszContentType.</param>
        /// <param name="ppStream">on SUCCESS, contains a new IStream reference.  Use this to retrieve the binary data.</param>
        /// <returns>
        /// <list type="Return Values">
        /// <item>S_OK - ppStream contains an IStream*.  Caller must release the refrence.</item>
        /// <item>
        /// S_FALSE - The binary data has been deleted.
        /// ppStream does not contain a reference.  pszContentType has been zeroed.
        /// </item>
        /// <item>HRESULT_FROM_WIN32(ERROR_PATH_NOT_FOUND) - no data found for this property name.</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_INVALID_DATATYPE) - unable to get this value for this
        /// property due to schema</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER) - pszValue was not large enough to
        /// store the value.  The required buffer size is stored in pdwcchContentTypeRequired.</item>
        /// <item>Other FAILED HRESULTs</item>
        /// </list>
        /// </returns>
        [PreserveSig]
        HRESULT GetBinary([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
                          [In] uint dwFlags,
                          [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszContentType,
                          [In] uint cchContentType,
                          [Out] out uint pdwcchContentTypeRequired,
                          [Out, MarshalAs(UnmanagedType.Interface)] out IStream ppStream);

        

        /// <summary>
        /// Retrieve the labels for a named array node.
        /// </summary>
        /// <remarks>
        /// Warning: pszLabels is a list of strings concatenated together,
        /// followed by an empty string.  When callers are parsing this, they must look for
        /// two adjacent null-terminating characters.
        /// This function may return labels in a different order than they were set in.
        /// </remarks>
        /// <param name="pszArrayElementName">Name of the property to retrieve the labels for.</param>
        /// <param name="dwFlags">Reserved.  Must be CGD_DEFAULT.</param>
        /// <param name="pszLabels">User allocated buffer to store the labels in.</param>
        /// <param name="cchLabels">Size reserved by the caller for pszLabels.</param>
        /// <param name="pdwcchLabelsRequired">on FAILURE, contains the required size for pszLabels.</param>
        /// <returns>
        /// <list type="Return Values">
        /// <item>S_OK - pszLabels contains the set of labels.</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_PATH_NOT_FOUND) - no data found for this property name.</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_INVALID_DATATYPE) - unable to get this value for this
        /// property due to schema</item>
        /// <item>HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER) - pszLabels was not large enough to
        /// store the value.  The required buffer size is stored in pdwcchLabelsRequired.</item>
        /// <item>Other FAILED HRESULTs</item>
        /// </list>
        /// </returns>
        [PreserveSig]
        HRESULT GetLabels([In, MarshalAs(UnmanagedType.LPWStr)] string pszArrayElementName,
                          [In] uint dwFlags,
                          [In, Out, MarshalAs(UnmanagedType.LPWStr)] IntPtr pszLabels,
                          [In] uint cchLabels,
                          [Out] out uint pdwcchLabelsRequired);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszPropertyName"></param>
        /// <param name="dwFlags"></param>
        /// <param name="pszValue"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT SetString([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
                          [In] uint dwFlags,
                          [In, MarshalAs(UnmanagedType.LPWStr)] string pszValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszPropertyName"></param>
        /// <param name="dwFlags"></param>
        /// <param name="ftDateTime"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT SetDate([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
                        [In] uint dwFlags,
                        [In] FILETIME ftDateTime);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszPropertyName"></param>
        /// <param name="dwFlags"></param>
        /// <param name="pszContentType"></param>
        /// <param name="pStream"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT SetBinary([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
                          uint dwFlags,
                          [In, MarshalAs(UnmanagedType.LPWStr)] string pszContentType,
                          [In, MarshalAs(UnmanagedType.Interface)] IStream pStream);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszArrayElementName"></param>
        /// <param name="dwFlags"></param>
        /// <param name="dwLabelCount"></param>
        /// <param name="ppszLabels"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT SetLabels([In, MarshalAs(UnmanagedType.LPWStr)] string pszArrayElementName,
                          [In] uint dwFlags,
                          [In] uint dwLabelCount,
                          [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2, ArraySubType=UnmanagedType.LPWStr)] IntPtr ppszLabels);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszArrayName"></param>
        /// <param name="dwFlags"></param>
        /// <param name="fAppend">
        /// A Boolean value exposed as an UInt32.  Acceptable values are 1 (TRUE) and 0 (FALSE).
        /// Experimentally, the Vista API have odd behavior when this is marshalled as a System.Boolean,
        /// as it only accepts those two values.
        /// </param>
        /// <param name="pszNewArrayElementName"></param>
        /// <param name="cchNewArrayElementName"></param>
        /// <param name="pdwcchNewArrayElementNameRequired"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT CreateArrayNode([In, MarshalAs(UnmanagedType.LPWStr)] string pszArrayName,
                                [In] uint dwFlags,
                                [In] uint fAppend,
                                [In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszNewArrayElementName,
                                [In] uint cchNewArrayElementName,
                                [Out] out uint pdwcchNewArrayElementNameRequired);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszPropertyName"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT DeleteProperty([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
                               [In] uint dwFlags);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszArrayElementName"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT DeleteArrayNode([In, MarshalAs(UnmanagedType.LPWStr)] string pszArrayElementName,
                                [In] uint dwFlags);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszArrayElementName"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT DeleteLabels([In, MarshalAs(UnmanagedType.LPWStr)] string pszArrayElementName,
                             [In] uint dwFlags);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ppPropertyCollection"></param>
        /// <param name="dwFlags"></param>
        /// <param name="pszMultiValueName"></param>
        /// <param name="dwLabelCount"></param>
        /// <param name="ppszLabels"></param>
        /// <param name="fAnyLabelMatches"></param>
        /// <returns></returns>
        [PreserveSig]
        HRESULT GetPropertyCollection([Out, MarshalAs(UnmanagedType.Interface)] out IContactPropertyCollection ppPropertyCollection,
                                      [In] uint dwFlags,
                                      [In, MarshalAs(UnmanagedType.LPWStr)] string pszMultiValueName,
                                      [In] uint dwLabelCount,
                                      [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPWStr, SizeParamIndex=3)] IntPtr ppszLabels,
                                      [In] uint fAnyLabelMatches);
    }

    #endregion

    #region IContact Class Declarations

	[
	ComImport,
	TypeLibType(TypeLibTypeFlags.FCanCreate),
	ClassInterface(ClassInterfaceType.None),
	Guid(CLSIDGuid.ContactManager),
	]
	public class ContactManagerRcw : IContactManager
	{
		#region IContactManager Members

		/// <exclude/>
		[PreserveSig,
		MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern HRESULT Initialize(string pszAppName, string pszAppVersion);

		/// <exclude/>
		[PreserveSig,
		MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern HRESULT Load(string pszContactID, out IContact ppContact);

		/// <exclude/>
		[PreserveSig,
		MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern HRESULT MergeContactIDs(string pszNewContactID, string pszOldContactID);

		/// <exclude/>
		[PreserveSig,
		MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern HRESULT GetMeContact(out IContact ppMeContact);

		/// <exclude/>
		[PreserveSig,
		MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern HRESULT SetMeContact(IContact pMeContact);

		/// <exclude/>
		[PreserveSig,
		MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern HRESULT GetContactCollection(out IContactCollection ppContactCollection);
		#endregion
	}

    /// <summary>
    /// Default implementation of the IContactManager interface via a runtime callable wrapper.
    /// </summary>
    /// <remarks>
    /// Please refer to the interfaces that this implements
    /// for documentation on the methods of this class.
    /// Non-contact related interfaces are documented on MSDN.
    /// </remarks>

	[
		ComImport,
		TypeLibType(TypeLibTypeFlags.FCanCreate),
		ClassInterface(ClassInterfaceType.None),
		Guid(CLSIDGuid.Contact),
    ]
    public class ContactRcw : IContact, INativeContactProperties, IPersistStream
    {
		[PreserveSig,
		MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern HRESULT GetContactID(StringBuilder pszContactID, uint cchContactID, out uint pdwcchContactIDRequired);

		[PreserveSig,
		MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern HRESULT GetPath(StringBuilder pszPath, uint cchPath, out uint pdwcchPathRequired);

		[PreserveSig,
		MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern HRESULT CommitChanges(uint dwCommitFlags);
		
        #region IContactProperties Members
        /// <exclude/>
        [PreserveSig,
        MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HRESULT GetString(string pszPropertyName, uint dwFlags, StringBuilder pszValue, uint cchValue, out uint pdwcchPropertyValueRequired);

        /// <exclude/>
        [PreserveSig,
        MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HRESULT GetDate(string pszPropertyName, uint dwFlags, out FILETIME pftDateTime);

        /// <exclude/>
        [PreserveSig,
        MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HRESULT GetBinary(string pszPropertyName, uint dwFlags, StringBuilder pszContentType, uint cchContentType, out uint pdwcchContentTypeRequired, out IStream ppStream);

        /// <exclude/>
        [PreserveSig,
        MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HRESULT GetLabels(string pszArrayElementName, uint dwFlags, IntPtr pszLabels, uint cchLabels, out uint pdwcchLabelsRequired);

        /// <exclude/>
        [PreserveSig,
        MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HRESULT SetString(string pszPropertyName, uint dwFlags, string pszValue);

        /// <exclude/>
        [PreserveSig,
        MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HRESULT SetDate(string pszPropertyName, uint dwFlags, FILETIME ftDateTime);

        /// <exclude/>
        [PreserveSig,
        MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HRESULT SetBinary(string pszPropertyName, uint dwFlags, string pszContentType, IStream pStream);

        /// <exclude/>
        [PreserveSig,
        MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HRESULT SetLabels(string pszArrayElementName, uint dwFlags, uint dwLabelCount, IntPtr ppszLabels);

        /// <exclude/>
        [PreserveSig,
        MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HRESULT CreateArrayNode(string pszArrayName, uint dwFlags, uint fAppend, StringBuilder pszNewArrayElementName, uint cchNewArrayElementName, out uint pdwcchNewArrayElementNameRequired);

        /// <exclude/>
        [PreserveSig,
        MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HRESULT DeleteProperty(string pszPropertyName, uint dwFlags);

        /// <exclude/>
        [PreserveSig,
        MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HRESULT DeleteArrayNode(string pszArrayElementName, uint dwFlags);

        /// <exclude/>
        [PreserveSig,
        MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HRESULT DeleteLabels(string pszArrayElementName, uint dwFlags);

        /// <exclude/>
        [PreserveSig,
        MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HRESULT GetPropertyCollection(out IContactPropertyCollection ppPropertyCollection, uint dwFlags, string pszMultiValueName, uint dwLabelCount, IntPtr ppszLabels, uint fAnyLabelMatches);
        #endregion

        #region IPersistStream Members
        /// <exclude/>
        [
            PreserveSig,
            MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)
        ]
        public virtual extern HRESULT Load(IStream pstm);

        /// <exclude/>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void Save(IStream pstm, bool fClearDirty);

        /// <exclude/>
        [
            MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime),
            Obsolete("The underlying COM object does not implement this method.", true)
        ]
        public virtual extern void GetSizeMax(out ulong pcbSize);
        #endregion

        #region IPersist Members

        /// <exclude/>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void GetClassID(out Guid pClassID);

        /// <exclude/>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HRESULT IsDirty();

        #endregion
    }

    #endregion
}