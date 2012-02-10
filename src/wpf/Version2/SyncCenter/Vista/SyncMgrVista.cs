// 
//  Copyright (c) Synrc Research Center.  All rights reserved. 
// 

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.SyncCenter
{
	[ComImport, Guid("A7F337A3-D20B-45CB-9ED7-87D094CA5045"),
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ISyncMgrHandlerCollection
	{
		int GetHandlerEnumerator([MarshalAs(UnmanagedType.Interface)] out IEnumString ppenum);
		int BindToHandler([In, MarshalAs(UnmanagedType.LPWStr)] string pszHandlerID, [In] ref Guid riid, out IntPtr ppv);
	}

	[ComImport(), Guid("884CCD87-B139-4937-A4BA-4F8E19513FBE"),
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)
	]
	public interface ISyncMgrSyncCallback
	{
	}

	[ComImport(), Guid("17F48517-F305-4321-A08D-B25A834918FD"),
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ISyncMgrSessionCreator
	{
		[PreserveSig]
		int CreateSession([In, MarshalAs(UnmanagedType.LPWStr)] string pszHandlerID,
			[In, MarshalAs(UnmanagedType.LPWStr)] ref string ppszItemIDs,
			[In] uint cItems,
			[MarshalAs(UnmanagedType.Interface)] out ISyncMgrSyncCallback ppCallback);
	}

	[ComImport(), Guid("90701133-BE32-4129-A65C-99E616CAFFF4"),
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ISyncMgrSyncItemContainer
	{
		int GetSyncItem([In, MarshalAs(UnmanagedType.LPWStr)] string pszItemID, [MarshalAs(UnmanagedType.Interface)] out ISyncMgrSyncItem ppItem);
		void GetSyncItemCount(out uint pcItems);
		void GetSyncItemEnumerator([MarshalAs(UnmanagedType.Interface)] out IEnumSyncMgrSyncItems ppenum);
	}

	[ComImport, Guid("54B3ABF3-F085-4181-B546-E29C403C726B"),
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumSyncMgrSyncItems
	{
		int Next([In] uint celt, [MarshalAs(UnmanagedType.Interface)] out ISyncMgrSyncItem rgelt, out uint pceltFetched);
		int Skip([In] uint celt);
		int Reset();
		int Clone([MarshalAs(UnmanagedType.Interface)] out IEnumSyncMgrSyncItems ppenum);
	}

	[ComImport, Guid("B20B24CE-2593-4F04-BD8B-7AD6C45051CD"),
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ISyncMgrSyncItem
	{
		int GetItemID([MarshalAs(UnmanagedType.LPWStr)] out string ppszItemID);
		int GetName([MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
		int GetItemInfo([MarshalAs(UnmanagedType.Interface)] out ISyncMgrSyncItemInfo ppItemInfo);
		int GetObject([In] ref Guid rguidObjectID, [In] ref Guid riid, out IntPtr ppv);
		int GetCapabilities(out SYNCMGR_ITEM_CAPABILITIES pmCapabilities);
		int GetPolicies(out SYNCMGR_ITEM_POLICIES pmPolicies);
		int Enable([In] int fEnable);
		int Delete();
	}

	[ComImport, Guid("E7FD9502-BE0C-4464-90A1-2B5277031232"),
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown),]
	public interface ISyncMgrSyncItemInfo
	{
		int GetTypeLabel([MarshalAs(UnmanagedType.LPWStr)] out string ppszTypeLabel);
		int GetComment([MarshalAs(UnmanagedType.LPWStr)] out string ppszComment);
		int GetLastSyncTime(out System.Runtime.InteropServices.ComTypes.FILETIME pftLastSync);
		int IsEnabled();
		int IsConnected();
	}

	[ComImport(), Guid("4FF1D798-ECF7-4524-AA81-1E362A0AEF3A"),
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ISyncMgrHandlerInfo
	{
		[PreserveSig]
		int GetComment(out string ppszComment);

		[PreserveSig]
		int GetLastSyncTime(out System.Runtime.InteropServices.ComTypes.FILETIME pftLastSync);

		[PreserveSig]
		int GetType(out SYNCMGR_HANDLER_TYPE pftLastSync);

		[PreserveSig]
		int GetTypeLabel(out string ppszTypeLabel);

		[PreserveSig]
		int IsActive();

		[PreserveSig]
		int IsConnected();

		[PreserveSig]
		int IsEnabled();
	}

	[ComImport(), Guid("04EC2E43-AC77-49F9-9B98-0307EF7A72A2"),
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
	public interface ISyncMgrHandler
	{
		[PreserveSig]
		int Activate([In] int fActivate);

		[PreserveSig]
		int Enable([In] int fActivate);

		[PreserveSig]
		int GetCapabilities(out SYNCMGR_HANDLER_CAPABILITIES pmCapabilities);

		[PreserveSig]
		int GetHandlerInfo([MarshalAs(UnmanagedType.Interface)] out ISyncMgrHandlerInfo ppHandlerInfo);

		[PreserveSig]
		int GetName([MarshalAs(UnmanagedType.LPWStr)] out string ppszName);

		[PreserveSig]
		int GetObject([In] ref Guid rguidObjectID, [In] ref Guid riid, out IntPtr ppv);

		[PreserveSig]
		int GetPolicies(out SYNCMGR_HANDLER_POLICIES pmPolicies);

		[PreserveSig]
		int Synchronize([In, MarshalAs(UnmanagedType.LPWStr)] ref string ppszItemIDs, [In] uint cItems, [In] ref IntPtr hwndOwner, [In, MarshalAs(UnmanagedType.Interface)] ISyncMgrSessionCreator pSessionCreator, [In, MarshalAs(UnmanagedType.IUnknown)] object punk);
	}

	public enum SYNCMGR_HANDLER_TYPE
	{
		Unknown = 0x0000,
		Application = 0x0001,
		Device = 0x0002,
		Folder = 0x0003,
		Service = 0x0004,
		Computer = 0x0005,
		Min = 0x0000,
		Max = Computer
	};

	public enum SYNCMGR_HANDLER_CAPABILITIES
	{
		SYNCMGR_HCM_NONE = 0x00000000,
		SYNCMGR_HCM_PROVIDES_ICON = 0x00000001,
		SYNCMGR_HCM_EVENT_STORE = 0x00000002,
		SYNCMGR_HCM_CONFLICT_STORE = 0x00000004,
		SYNCMGR_HCM_SUPPORTS_CONCURRENT_SESSIONS = 0x00000010,
		SYNCMGR_HCM_CAN_BROWSE_CONTENT = 0x00010000,
		SYNCMGR_HCM_CAN_SHOW_SCHEDULE = 0x00020000,
		SYNCMGR_HCM_QUERY_BEFORE_ACTIVATE = 0x00100000,
		SYNCMGR_HCM_QUERY_BEFORE_DEACTIVATE = 0x00200000,
		SYNCMGR_HCM_QUERY_BEFORE_ENABLE = 0x00400000,
		SYNCMGR_HCM_QUERY_BEFORE_DISABLE = 0x00800000,
		SYNCMGR_HCM_VALID_MASK = 0x00F3003F
	}

	public enum SYNCMGR_HANDLER_POLICIES
	{
		SYNCMGR_HPM_NONE = 0x00000000,
		SYNCMGR_HPM_PREVENT_ACTIVATE = 0x00000001,
		SYNCMGR_HPM_PREVENT_DEACTIVATE = 0x00000002,
		SYNCMGR_HPM_PREVENT_ENABLE = 0x00000004,
		SYNCMGR_HPM_PREVENT_DISABLE = 0x00000008,
		SYNCMGR_HPM_PREVENT_START_SYNC = 0x00000010,
		SYNCMGR_HPM_PREVENT_STOP_SYNC = 0x00000020,
		SYNCMGR_HPM_DISABLE_ENABLE = 0x00000100,
		SYNCMGR_HPM_DISABLE_DISABLE = 0x00000200,
		SYNCMGR_HPM_DISABLE_START_SYNC = 0x00000400,
		SYNCMGR_HPM_DISABLE_STOP_SYNC = 0x00000800,
		SYNCMGR_HPM_DISABLE_BROWSE = 0x00001000,
		SYNCMGR_HPM_DISABLE_SCHEDULE = 0x00002000,
		SYNCMGR_HPM_HIDDEN_BY_DEFAULT = 0x00010000,
		SYNCMGR_HCM_BACKGROUND_SYNC_ONLY,
		SYNCMGR_HCM_VALID_MASK
	}

	public enum SYNCMGR_ITEM_POLICIES
	{
		SYNCMGR_IPM_DISABLE_BROWSE = 0x100,
		SYNCMGR_IPM_DISABLE_DELETE = 0x200,
		SYNCMGR_IPM_DISABLE_DISABLE = 0x20,
		SYNCMGR_IPM_DISABLE_ENABLE = 0x10,
		SYNCMGR_IPM_DISABLE_START_SYNC = 0x40,
		SYNCMGR_IPM_DISABLE_STOP_SYNC = 0x80,
		SYNCMGR_IPM_HIDDEN_BY_DEFAULT = 0x10000,
		SYNCMGR_IPM_NONE = 0,
		SYNCMGR_IPM_PREVENT_DISABLE = 2,
		SYNCMGR_IPM_PREVENT_ENABLE = 1,
		SYNCMGR_IPM_PREVENT_START_SYNC = 4,
		SYNCMGR_IPM_PREVENT_STOP_SYNC = 8,
		SYNCMGR_IPM_VALID_MASK = 0x102ff
	}

	public enum SYNCMGR_ITEM_CAPABILITIES
	{
		SYNCMGR_ICM_CAN_BROWSE_CONTENT = 0x10000,
		SYNCMGR_ICM_CAN_DELETE = 0x10,
		SYNCMGR_ICM_CONFLICT_STORE = 4,
		SYNCMGR_ICM_EVENT_STORE = 2,
		SYNCMGR_ICM_NONE = 0,
		SYNCMGR_ICM_PROVIDES_ICON = 1,
		SYNCMGR_ICM_QUERY_BEFORE_DELETE = 0x400000,
		SYNCMGR_ICM_QUERY_BEFORE_DISABLE = 0x200000,
		SYNCMGR_ICM_QUERY_BEFORE_ENABLE = 0x100000,
		SYNCMGR_ICM_VALID_MASK = 0x710017
	}

}
