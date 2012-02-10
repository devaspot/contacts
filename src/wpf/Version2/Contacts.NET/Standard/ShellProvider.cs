/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Standard
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;

    using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

    #region Enums and Static Property Classes

    /// <summary>
    /// DATAOBJ_GET_ITEM_FLAGS.  DOGIF_*.
    /// </summary>
    internal enum DOGIF
    {
        DEFAULT = 0x0000,
        TRAVERSE_LINK = 0x0001,    // if the item is a link get the target
        NO_HDROP = 0x0002,    // don't fallback and use CF_HDROP clipboard format
        NO_URL = 0x0004,    // don't fallback and use URL clipboard format
        ONLY_IF_ONE = 0x0008,    // only return the item if there is one item in the array
    }

    // The following are new for Vista, but are use in downlevel components
    internal enum GETPROPERTYSTOREFLAGS
    {
        // If no flags are specified (GPS_DEFAULT), a read-only property store is returned that includes properties for the file or item.
        // In the case that the shell item is a file, the property store contains:
        //     1. properties about the file from the file system
        //     2. properties from the file itself provided by the file's property handler, unless that file is offline,
        //         see GPS_OPENSLOWITEM
        //     3. if requested by the file's property handler and supported by the file system, properties stored in the
        //     alternate property store.
        //
        // Non-file shell items should return a similar read-only store
        //
        // Specifying other GPS_ flags modifies the store that is returned
        GPS_DEFAULT = 0x00000000,
        GPS_HANDLERPROPERTIESONLY = 0x00000001,   // only include properties directly from the file's property handler
        GPS_READWRITE = 0x00000002,   // Writable stores will only include handler properties
        GPS_TEMPORARY = 0x00000004,   // A read/write store that only holds properties for the lifetime of the IShellItem object
        GPS_FASTPROPERTIESONLY = 0x00000008,   // do not include any properties from the file's property handler (because the file's property handler will hit the disk)
        GPS_OPENSLOWITEM = 0x00000010,   // include properties from a file's property handler, even if it means retrieving the file from offline storage.
        GPS_DELAYCREATION = 0x00000020,   // delay the creation of the file's property handler until those properties are read, written, or enumerated
        GPS_BESTEFFORT = 0x00000040,   // For readonly stores, succeed and return all available properties, even if one or more sources of properties fails. Not valid with GPS_READWRITE.
        GPS_NO_OPLOCK = 0x00000080,   // some data sources protect the read property store with an oplock, this disables that
        GPS_MASK_VALID = 0x000000FF,
    }

    internal enum KNOWNDESTCATEGORY
    {
        KDC_FREQUENT = 1,
        KDC_RECENT,
    }

    // IShellFolder::GetAttributesOf flags
    [Flags]
    internal enum SFGAO : uint
    {
        /// <summary>Objects can be copied</summary>
        /// <remarks>DROPEFFECT_COPY</remarks>
        CANCOPY = 0x1,
        /// <summary>Objects can be moved</summary>
        /// <remarks>DROPEFFECT_MOVE</remarks>
        CANMOVE = 0x2,
        /// <summary>Objects can be linked</summary>
        /// <remarks>
        /// DROPEFFECT_LINK.
        /// 
        /// If this bit is set on an item in the shell folder, a
        /// 'Create Shortcut' menu item will be added to the File
        /// menu and context menus for the item.  If the user selects
        /// that command, your IContextMenu::InvokeCommand() will be called
        /// with 'link'.
        /// That flag will also be used to determine if 'Create Shortcut'
        /// should be added when the item in your folder is dragged to another
        /// folder.
        /// </remarks>
        CANLINK = 0x4,
        /// <summary>supports BindToObject(IID_IStorage)</summary>
        STORAGE = 0x00000008,
        /// <summary>Objects can be renamed</summary>
        CANRENAME = 0x00000010,
        /// <summary>Objects can be deleted</summary>
        CANDELETE = 0x00000020,
        /// <summary>Objects have property sheets</summary>
        HASPROPSHEET = 0x00000040,

        // unused = 0x00000080,

        /// <summary>Objects are drop target</summary>
        DROPTARGET = 0x00000100,
        CAPABILITYMASK = 0x00000177,
        // unused = 0x00000200,
        // unused = 0x00000400,
        // unused = 0x00000800,
        // unused = 0x00001000,
        /// <summary>Object is encrypted (use alt color)</summary>
        ENCRYPTED = 0x00002000,
        /// <summary>'Slow' object</summary>
        ISSLOW = 0x00004000,
        /// <summary>Ghosted icon</summary>
        GHOSTED = 0x00008000,
        /// <summary>Shortcut (link)</summary>
        LINK = 0x00010000,
        /// <summary>Shared</summary>
        SHARE = 0x00020000,
        /// <summary>Read-only</summary>
        READONLY = 0x00040000,
        /// <summary> Hidden object</summary>
        HIDDEN = 0x00080000,
        DISPLAYATTRMASK = 0x000FC000,
        /// <summary> May contain children with SFGAO_FILESYSTEM</summary>
        FILESYSANCESTOR = 0x10000000,
        /// <summary>Support BindToObject(IID_IShellFolder)</summary>
        FOLDER = 0x20000000,
        /// <summary>Is a win32 file system object (file/folder/root)</summary>
        FILESYSTEM = 0x40000000,
        /// <summary>May contain children with SFGAO_FOLDER (may be slow)</summary>
        HASSUBFOLDER = 0x80000000,
        CONTENTSMASK = 0x80000000,
        /// <summary>Invalidate cached information (may be slow)</summary>
        VALIDATE = 0x01000000,
        /// <summary>Is this removeable media?</summary>
        REMOVABLE = 0x02000000,
        /// <summary> Object is compressed (use alt color)</summary>
        COMPRESSED = 0x04000000,
        /// <summary>Supports IShellFolder, but only implements CreateViewObject() (non-folder view)</summary>
        BROWSABLE = 0x08000000,
        /// <summary>Is a non-enumerated object (should be hidden)</summary>
        NONENUMERATED = 0x00100000,
        /// <summary>Should show bold in explorer tree</summary>
        NEWCONTENT = 0x00200000,
        /// <summary>Obsolete</summary>
        CANMONIKER = 0x00400000,
        /// <summary>Obsolete</summary>
        HASSTORAGE = 0x00400000,
        /// <summary>Supports BindToObject(IID_IStream)</summary>
        STREAM = 0x00400000,
        /// <summary>May contain children with SFGAO_STORAGE or SFGAO_STREAM</summary>
        STORAGEANCESTOR = 0x00800000,
        /// <summary>For determining storage capabilities, ie for open/save semantics</summary>
        STORAGECAPMASK = 0x70C50008,
        /// <summary>
        /// Attributes that are masked out for PKEY_SFGAOFlags because they are considered
        /// to cause slow calculations or lack context
        /// (SFGAO_VALIDATE | SFGAO_ISSLOW | SFGAO_HASSUBFOLDER and others)
        /// </summary>
        PKEYSFGAOMASK = 0x81044000,
    }

    /// <summary>
    /// IShellFolder::EnumObjects grfFlags bits.  Also called SHCONT
    /// </summary>
    internal enum SHCONTF
    {
        CHECKING_FOR_CHILDREN = 0x0010,   // hint that client is checking if (what) child items the folder contains - not all details (e.g. short file name) are needed
        FOLDERS = 0x0020,   // only want folders enumerated (SFGAO_FOLDER)
        NONFOLDERS = 0x0040,   // include non folders (items without SFGAO_FOLDER)
        INCLUDEHIDDEN = 0x0080,   // show items normally hidden (items with SFGAO_HIDDEN)
        INIT_ON_FIRST_NEXT = 0x0100,   // DEFUNCT - this is always assumed
        NETPRINTERSRCH = 0x0200,   // hint that client is looking for printers
        SHAREABLE = 0x0400,   // hint that client is looking sharable resources (local drives or hidden root shares)
        STORAGE = 0x0800,   // include all items with accessible storage and their ancestors
        NAVIGATION_ENUM = 0x1000,   // mark child folders to indicate that they should provide a "navigation" enumeration by default
        FASTITEMS = 0x2000,   // hint that client is only interested in items that can be enumerated quickly
        FLATLIST = 0x4000,   // enumerate items as flat list even if folder is stacked
        ENABLE_ASYNC = 0x8000,   // inform enumerator that client is listening for change notifications so enumerator does not need to be complete, items can be reported via change notifications
    }

    /// <summary>
    /// IShellFolder::GetDisplayNameOf/SetNameOf uFlags.  Also called SHGDNF.
    /// </summary>
    /// <remarks>
    /// For compatibility with SIGDN, these bits must all sit in the LOW word.
    /// </remarks>
    [Flags]
    internal enum SHGDN
    {
        SHGDN_NORMAL = 0x0000,  // default (display purpose)
        SHGDN_INFOLDER = 0x0001,  // displayed under a folder (relative)
        SHGDN_FOREDITING = 0x1000,  // for in-place editing
        SHGDN_FORADDRESSBAR = 0x4000,  // UI friendly parsing name (remove ugly stuff)
        SHGDN_FORPARSING = 0x8000,  // parsing name for ParseDisplayName()
    }

    /// <summary>
    /// SHELLITEMCOMPAREHINTF.  SICHINT_*.
    /// </summary>
    internal enum SICHINT : uint
    {
        /// <summary>iOrder based on display in a folder view</summary>
        DISPLAY = 0x00000000,
        /// <summary>exact instance compare</summary>
        ALLFIELDS = 0x80000000,
        /// <summary>iOrder based on canonical name (better performance)</summary>
        CANONICAL = 0x10000000,
        TEST_FILESYSPATH_IF_NOT_EQUAL = 0x20000000,
    };

    /// <summary>
    /// ShellItem enum.  SIGDN_*.
    /// </summary>
    internal enum SIGDN : uint
    {                                             // lower word (& with 0xFFFF)
        NORMALDISPLAY = 0x00000000, // SHGDN_NORMAL
        PARENTRELATIVEPARSING = 0x80018001, // SHGDN_INFOLDER | SHGDN_FORPARSING
        DESKTOPABSOLUTEPARSING = 0x80028000, // SHGDN_FORPARSING
        PARENTRELATIVEEDITING = 0x80031001, // SHGDN_INFOLDER | SHGDN_FOREDITING
        DESKTOPABSOLUTEEDITING = 0x8004c000, // SHGDN_FORPARSING | SHGDN_FORADDRESSBAR
        FILESYSPATH = 0x80058000, // SHGDN_FORPARSING
        URL = 0x80068000, // SHGDN_FORPARSING
        PARENTRELATIVEFORADDRESSBAR = 0x8007c001, // SHGDN_INFOLDER | SHGDN_FORPARSING | SHGDN_FORADDRESSBAR
        PARENTRELATIVE = 0x80080001, // SHGDN_INFOLDER
    }

    /// <summary>
    /// STR_GPS_*
    /// </summary>
    /// <remarks>
    /// When requesting a property store through IShellFolder, you can specify the equivalent of
    /// GPS_DEFAULT by passing in a null IBindCtx parameter.
    ///
    /// You can specify the equivalent of GPS_READWRITE by passing a mode of STGM_READWRITE | STGM_EXCLUSIVE
    /// in the bind context
    ///
    /// Here are the string versions of GPS_ flags, passed to IShellFolder::BindToObject() via IBindCtx::RegisterObjectParam()
    /// These flags are valid when requesting an IPropertySetStorage or IPropertyStore handler
    ///
    /// The meaning of these flags are described above.
    ///
    /// There is no STR_ equivalent for GPS_TEMPORARY because temporary property stores
    /// are provided by IShellItem2 only -- not by the underlying IShellFolder.
    /// </remarks>
    internal static class STR_GPS
    {
        public const string HANDLERPROPERTIESONLY = "GPS_HANDLERPROPERTIESONLY";
        public const string FASTPROPERTIESONLY = "GPS_FASTPROPERTIESONLY";
        public const string OPENSLOWITEM = "GPS_OPENSLOWITEM";
        public const string DELAYCREATION = "GPS_DELAYCREATION";
        public const string BESTEFFORT = "GPS_BESTEFFORT";
        public const string NO_OPLOCK = "GPS_NO_OPLOCK";
    }

    #endregion

    #region Structs

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct PKEY
    {
        /// <summary>fmtid</summary>
        private readonly Guid _fmtid;
        /// <summary>pid</summary>
        private readonly uint _pid;

        public PKEY(Guid fmtid, uint pid)
        {
            _fmtid = fmtid;
            _pid = pid;
        }

        /// <summary>PKEY_Title</summary>
        public static readonly PKEY Title = new PKEY(new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), 2);
        /// <summary>PKEY_AppUserModel_ID</summary>
        public static readonly PKEY AppUserModel_ID = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);
        /// <summary>PKEY_AppUserModel_IsDestListSeparator</summary>
        public static readonly PKEY AppUserModel_IsDestListSeparator = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 6);
        /// <summary>PKEY_AppUserModel_RelaunchCommand</summary>
        public static readonly PKEY AppUserModel_RelaunchCommand = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 2);
        /// <summary>PKEY_AppUserModel_RelaunchDisplayNameResource</summary>
        public static readonly PKEY AppUserModel_RelaunchDisplayNameResource = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 4);
        /// <summary>PKEY_AppUserModel_RelaunchIconResource</summary>
        public static readonly PKEY AppUserModel_RelaunchIconResource = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 3);
    }

    #endregion

    #region Interfaces

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.EnumIdList),
    ]
    internal interface IEnumIDList
    {
        [PreserveSig()]
        HRESULT Next(uint celt, out IntPtr rgelt, out int pceltFetched);
        [PreserveSig()]
        HRESULT Skip(uint celt);
        void Reset();
        void Clone([Out, MarshalAs(UnmanagedType.Interface)] out IEnumIDList ppenum);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.EnumObjects),
    ]
    internal interface IEnumObjects
    {
        //[local]
        // This probably doesn't work... Hopefully don't need this interface though.k
        void Next([In] uint celt, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown, IidParameterIndex = 1, SizeParamIndex = 0)] object[] rgelt, [Out] out uint pceltFetched);

        /*
        [call_as(Next)] HRESULT RemoteNext(
            [in] ULONG celt,
            [in] REFIID riid,
            [out, size_is(celt), length_is(*pceltFetched), iid_is(riid)] void **rgelt,
            [out] ULONG *pceltFetched);
         */

        void Skip([In] uint celt);

        void Reset();

        void Clone([Out] out IEnumObjects ppenum);
    }

    /// <summary>Unknown Object Array</summary>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.ObjectArray),
    ]
    internal interface IObjectArray
    {
        void GetCount([Out] out uint pcObjects);
        void GetAt([In] uint uiIndex, [In] ref Guid riid, [Out] out object ppv);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.ObjectArray),
    ]
    interface IObjectCollection : IObjectArray
    {
        #region IObjectArray redeclarations
        new void GetCount([Out] out uint pcObjects);
        new void GetAt([In] uint uiIndex, [In] ref Guid riid, [Out] out object ppv);
        #endregion

        void AddObject([In, MarshalAs(UnmanagedType.IUnknown)] object punk);
        void AddFromArray([In] IObjectArray poaSource);
        void RemoveObjectAt([In] uint uiIndex);
        void Clear();
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.PropertyStore)
    ]
    internal interface IPropertyStore
    {
        void GetCount(out uint cProps);
        void GetAt(uint iProp, [MarshalAs(UnmanagedType.Struct)] out PKEY pkey);
        void GetValue([In, MarshalAs(UnmanagedType.Struct)] ref PKEY pkey, [Out, MarshalAs(UnmanagedType.Struct)] out PROPVARIANT pv);
        void SetValue([In, MarshalAs(UnmanagedType.Struct)] ref PKEY pkey, [In, MarshalAs(UnmanagedType.Struct)] ref PROPVARIANT pv);
        void Commit();
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.ShellFolder),
    ]
    internal interface IShellFolder
    {
        void ParseDisplayName(
            [In] IntPtr hwnd,
            [In] IBindCtx pbc,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
            [In, Out] ref int pchEaten,
            [Out] out IntPtr ppidl,
            [In, Out] ref uint pdwAttributes);

        void EnumObjects(
            [In] IntPtr hwnd,
            [In] SHCONTF grfFlags,
            [Out] out IEnumIDList ppenumIDList);

        // returns an instance of a sub-folder which is specified by the IDList (pidl).
        // IShellFolder or derived interfaces
        void BindToObject(
            [In] IntPtr pidl,
            [In] IBindCtx pbc,
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

        // produces the same result as BindToObject()
        void BindToStorage([In] IntPtr pidl, [In] IBindCtx pbc, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

        // compares two IDLists and returns the result. The shell
        // explorer always passes 0 as lParam, which indicates 'sort by name'.
        // It should return 0 (as CODE of the scode), if two id indicates the
        // same object; negative value if pidl1 should be placed before pidl2;
        // positive value if pidl2 should be placed before pidl1.
        // use the macro ResultFromShort() to extract the result comparison
        // it deals with the casting and type conversion issues for you
        [PreserveSig]
        HRESULT CompareIDs([In] IntPtr lParam, [In] IntPtr pidl1, [In] IntPtr pidl2);

        // creates a view object of the folder itself. The view
        // object is a difference instance from the shell folder object.
        // 'hwndOwner' can be used  as the owner window of its dialog box or
        // menu during the lifetime of the view object.
        // This member function should always create a new
        // instance which has only one reference count. The explorer may create
        // more than one instances of view object from one shell folder object
        // and treat them as separate instances.
        // returns IShellView derived interface
        void CreateViewObject([In] IntPtr hwndOwner, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

        // returns the attributes of specified objects in that
        // folder. 'cidl' and 'apidl' specifies objects. 'apidl' contains only
        // simple IDLists. The explorer initializes *prgfInOut with a set of
        // flags to be evaluated. The shell folder may optimize the operation
        // by not returning unspecified flags.
        void GetAttributesOf(
            [In] uint cidl,
            [In] IntPtr apidl,
            [In, Out] ref SFGAO rgfInOut);

        // creates a UI object to be used for specified objects.
        // The shell explorer passes either IID_IDataObject (for transfer operation)
        // or IID_IContextMenu (for context menu operation) as riid
        // and many other interfaces
        void GetUIObjectOf(
            [In] IntPtr hwndOwner,
            [In] uint cidl,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysInt, SizeParamIndex = 2)] IntPtr apidl,
            [In] ref Guid riid,
            [In, Out] ref uint rgfReserved,
            [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

        // returns the display name of the specified object.
        // If the ID contains the display name (in the locale character set),
        // it returns the offset to the name. Otherwise, it returns a pointer
        // to the display name string (UNICODE), which is allocated by the
        // task allocator, or fills in a buffer.
        // use the helper APIS StrRetToStr() or StrRetToBuf() to deal with the different
        // forms of the STRRET structure
        void GetDisplayNameOf([In] IntPtr pidl, [In] SHGDN uFlags, [Out] out IntPtr pName);

        // sets the display name of the specified object.
        // If it changes the ID as well, it returns the new ID which is
        // alocated by the task allocator.
        void SetNameOf([In] IntPtr hwnd,
            [In] IntPtr pidl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
            [In] SHGDN uFlags,
            [Out] out IntPtr ppidlOut);
    }

    /// <summary>
    /// Shell Namespace helper
    /// </summary>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.ShellItem),
    ]
    internal interface IShellItem
    {
        void BindToHandler([In] IBindCtx pbc, [In] ref Guid bhid, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

        void GetParent([Out, MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

        void GetDisplayName([In] SIGDN sigdnName, [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszName);

        void GetAttributes([In] SFGAO sfgaoMask, [Out] out SFGAO psfgaoAttribs);

        void Compare([In] IShellItem psi, [In] SICHINT hint, [Out] out int piOrder);
    }

    /// <summary>
    /// Shell Namespace helper 2
    /// </summary>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.ShellItem2),
    ]
    interface IShellItem2 : IShellItem
    {
        #region IShellItem redeclarations
        new void BindToHandler([In] IBindCtx pbc, [In] ref Guid bhid, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);
        new void GetParent([Out, MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);
        new void GetDisplayName([In] SIGDN sigdnName, [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
        new void GetAttributes([In] SFGAO sfgaoMask, [Out] out SFGAO psfgaoAttribs);
        new void Compare([In] IShellItem psi, [In] SICHINT hint, [Out] out int piOrder);
        #endregion

        void GetPropertyStore(
            [In] GETPROPERTYSTOREFLAGS flags,
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

        void GetPropertyStoreWithCreateObject(
            [In] GETPROPERTYSTOREFLAGS flags,
            [In, MarshalAs(UnmanagedType.IUnknown)] object punkCreateObject,   // factory for low-rights creation of type ICreateObject
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

        void GetPropertyStoreForKeys(
            [In] IntPtr rgKeys,
            [In] uint cKeys,
            [In] GETPROPERTYSTOREFLAGS flags,
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

        void GetPropertyDescriptionList(
            [In] IntPtr keyType,
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

        // Ensures any cached information in this item is up to date, or returns __HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND) if the item does not exist.
        void Update([In] IBindCtx pbc);

        void GetProperty(
            [In] IntPtr key,
            [Out] out object ppropvar);

        void GetCLSID(
            [In] IntPtr key,
            [Out] out Guid pclsid);

        void GetFileTime(
            [In] IntPtr key,
            [Out] FILETIME pft);

        void GetInt32(
            [In] IntPtr key,
            [Out] out int pi);

        void GetString(
            [In] IntPtr key,
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppsz);

        void GetUInt32(
            [In] IntPtr key,
            [Out] out uint pui);

        void GetUInt64(
            [In] IntPtr key,
            [Out] out ulong pull);

        void GetBool(
            [In] IntPtr key,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool pf);
    }

    [
        ComImport,
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.ShellLink),
    ]
    internal interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, [In, Out] WIN32_FIND_DATAW pfd, uint fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotKey(out short pwHotKey);
        void SetHotKey(short wHotKey);
        void GetShowCmd(out uint piShowCmd);
        void SetShowCmd(uint iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
        void Resolve(IntPtr hwnd, uint fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.TaskbarList),
    ]
    internal interface ITaskbarList
    {
        /// <summary>
        /// This function must be called first to validate use of other members.
        /// </summary>
        void HrInit();

        /// <summary>
        /// This function adds a tab for hwnd to the taskbar.
        /// </summary>
        /// <param name="hwnd">The HWND for which to add the tab.</param>
        void AddTab([In] IntPtr hwnd);

        /// <summary>
        /// This function deletes a tab for hwnd from the taskbar.
        /// </summary>
        /// <param name="hwnd">The HWND for which the tab is to be deleted.</param>
        void DeleteTab([In] IntPtr hwnd);

        /// <summary>
        /// This function activates the tab associated with hwnd on the taskbar.
        /// </summary>
        /// <param name="hwnd">The HWND for which the tab is to be actuvated.</param>
        void ActivateTab([In] IntPtr hwnd);

        /// <summary>
        /// This function marks hwnd in the taskbar as the active tab.
        /// </summary>
        /// <param name="hwnd">The HWND to activate.</param>
        void SetActiveAlt([In] IntPtr hwnd);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IIDGuid.TaskbarList2),
    ]
    internal interface ITaskbarList2 : ITaskbarList
    {
        #region ITaskbarList redeclaration
        new void HrInit();
        new void AddTab([In] IntPtr hwnd);
        new void DeleteTab([In] IntPtr hwnd);
        new void ActivateTab([In] IntPtr hwnd);
        new void SetActiveAlt([In] IntPtr hwnd);
        #endregion

        /// <summary>
        /// Marks a window as full-screen.
        /// </summary>
        /// <param name="hwnd">The handle of the window to be marked.</param>
        /// <param name="fFullscreen">A Boolean value marking the desired full-screen status of the window.</param>
        /// <remarks>
        /// Setting the value of fFullscreen to true, the Shell treats this window as a full-screen window, and the taskbar
        /// is moved to the bottom of the z-order when this window is active.  Setting the value of fFullscreen to false
        /// removes the full-screen marking, but <i>does not</i> cause the Shell to treat the window as though it were
        /// definitely not full-screen.  With a false fFullscreen value, the Shell depends on its automatic detection facility
        /// to specify how the window should be treated, possibly still flagging the window as full-screen.
        /// </remarks>
        void MarkFullscreenWindow([In] IntPtr hwnd, [In, MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
    }

    #endregion

    #region Runtime Callable Wrappers

    [
        ComImport,
        TypeLibType(TypeLibTypeFlags.FCanCreate),
        ClassInterface(ClassInterfaceType.None),
        Guid(CLSIDGuid.TaskbarList),
    ]
    internal class TaskbarListRcw
    {
    }

    [
        ComImport,
        TypeLibType(TypeLibTypeFlags.FCanCreate),
        ClassInterface(ClassInterfaceType.None),
        Guid(CLSIDGuid.EnumerableObjectCollection),
    ]
    internal class EnumerableObjectCollectionRcw
    {
    }

    [
        ComImport,
        TypeLibType(TypeLibTypeFlags.FCanCreate),
        ClassInterface(ClassInterfaceType.None),
        Guid(CLSIDGuid.ShellLink),
    ]
    internal class ShellLinkRcw
    {
    }
    
    #endregion
}
