// 
//  Copyright (c) Synrc Research Center.  All rights reserved. 
// 

using System;
using System.Runtime.InteropServices;

namespace Microsoft.SyncCenter
{

	[ComImport(), Guid("6295DF40-35EE-11d1-8707-00C04FD93327"),	
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ISyncMgrSynchronize
	{
		[PreserveSig]
		int Initialize(int dwReserved, int dwSyncMgrFlags, int cbCookie, IntPtr lpCookie);
	
		[PreserveSig]
		int GetHandlerInfo(out SyncMgrHandlerInfo ppSyncMgrHandlerInfo);
	
		[PreserveSig]
		int EnumSyncMgrItems(out ISyncMgrEnumItems ppSyncMgrEnumItems);

		[PreserveSig]
		int GetItemObject(ref Guid ItemID, ref Guid riid, out IntPtr ppv);

		[PreserveSig]
		int ShowProperties(IntPtr hWndParent, ref Guid ItemID);

		[PreserveSig]
		int SetProgressCallback(ISyncMgrSynchronizeCallback lpCallBack);

		[PreserveSig]
		int PrepareForSync(
			int cbNumItems,
			[MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] Guid[] pItemIDs,
			IntPtr hWndParent,
			int dwReserved);

		[PreserveSig]
		int Synchronize(IntPtr hWndParent);

		[PreserveSig]
		int SetItemStatus(ref Guid pItemID,	int dwSyncMgrStatus);

		[PreserveSig]
		int ShowError(IntPtr hWndParent, ref Guid ErrorID);
	}

	[ComVisible(false)]
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
	public class SyncMgrHandlerInfo 
	{
		public int cbSize = 0;
		public IntPtr hIcon = IntPtr.Zero;
		public int SyncMgrHandlerFlags = 0;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=32)] 
		public char[] wszHandlerName = new char[32];
	}

	public enum SyncMgrFlag
	{   
		Connect = 0x0001,
		PendingDisconnect = 0x0002,
		Manual = 0x0003,
		Idle = 0x0004,
		Invoke = 0x0005,
		Scheduled = 0x0006,
		EventMask = 0x00FF,
		Settings = 0x0100,
		MayBotherUser = 0x0200,
	}

	[FlagsAttribute] 
	public enum SyncMgrHandlerFlags
	{   
		HasProperties = 0x01,
		MayEstablishConnection = 0x02,
		AlwaysListHandler  = 0x04,
	}

	public enum SyncMgrStatus
	{
		Stopped = 0x0000,
		Skipped = 0x0001,
		Pending = 0x0002,
		Updating = 0x0003,
		Succeeded = 0x0004,
		Failed = 0x0005,
		Paused = 0x0006,
		Resuming = 0x0007,
		Deleted = 0x0100,
	}

	[ComImport(), Guid("6295DF2A-35EE-11d1-8707-00C04FD93327"),	
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ISyncMgrEnumItems
	{
		[PreserveSig]
		int Next(int celt, out SyncMgrItem syncMgrItem, out int pceltFetched);
 
		[PreserveSig]
		int Skip(int celt);

		[PreserveSig]
		int Reset();

		[PreserveSig]
		int Clone([MarshalAs(UnmanagedType.Interface)] out ISyncMgrEnumItems ppenum);
	}

	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
	public struct SyncMgrItem
	{
		public int cbSize;
		public int dwFlags;
		public Guid ItemID;
		public int dwItemState;
		public IntPtr hIcon;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=128)]
		public char[] wszItemName;
		public FILETIME ftLastUpdate;
	}

	[ComImport(), Guid("6295DF2C-35EE-11d1-8707-00C04FD93327"),	
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ISyncMgrSynchronizeInvoke
	{
		[PreserveSig]
		int UpdateItems(int dwInvokeFlags, ref Guid rclsid, int cbCookie, IntPtr lpCookie);

		[PreserveSig]
		int UpdateAll();
	}

	[ComImport(), Guid("6295DF42-35EE-11D1-8707-00C04FD93327"),	
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ISyncMgrRegister
	{
		[PreserveSig]
		int RegisterSyncMgrHandler(ref Guid rclsidHandler, 
			[MarshalAs(UnmanagedType.LPWStr)] string pwszDescription, 
			int dwSyncMgrRegisterFlags);

		[PreserveSig]
		int UnregisterSyncMgrHandler(ref Guid rclsidHandler, int dwReserved);

		[PreserveSig]
		int GetHandlerRegistrationInfo(ref Guid rclsidHandler, ref int pdwSyncMgrRegisterFlags);
	}

	[ComImport(), Guid("6295DF41-35EE-11d1-8707-00C04FD93327"),
	InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ISyncMgrSynchronizeCallback
	{
		[PreserveSig]
		int ShowPropertiesCompleted(int hr);

		[PreserveSig]
		int PrepareForSyncCompleted(int hr);

		[PreserveSig]
		int SynchronizeCompleted(int hr);

		[PreserveSig]
		int ShowErrorCompleted(	int hr,	int cbNumItems,
			[MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] Guid[] pItemIDs);

		[PreserveSig]
		int EnableModeless(int fEnable);

		[PreserveSig]
		int Progress(ref Guid pItemID, ref SyncMgrProgressItem lpSyncProgressItem);

		[PreserveSig]
		int LogError(int dwErrorLevel, string lpcErrorText,	ref SyncMgrLogErrorInfo lpSyncLogError);

		[PreserveSig]
		int DeleteLogError(ref Guid ErrorID, int dwReserved);

		[PreserveSig]
		int EstablishConnection(string lpwszConnection,	int dwReserved);
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SyncMgrProgressItem
	{
		public int cbSize;
		public int mask;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpcStatusText;
		public int dwStatusType;
		public int iProgValue;
		public int iMaxValue;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SyncMgrLogErrorInfo
	{
		public int cbSize;
		public int mask;
		public int dwSyncMgrErrorFlags;
		public Guid ErrorID;
		public Guid ItemID;
	}

	public enum SyncMgrErrorInfoMask
	{
		ErrorFlags = 0x0001,
		ErrorId = 0x0002,
		ItemId = 0x0004
	}

	public enum SyncMgrLogLevel
	{
		Information = 0x0001,
		Warning = 0x0002,
		Error = 0x0003
	}

	public enum SyncmgrErrorFlags
	{
		EnableJumpText = 0x01,   
	}

	[FlagsAttribute] 
	public enum StatusType
	{
		StatusText = 0x0001,
		StatusType = 0x0002,
		ProgValue = 0x0004,
		MaxValue = 0x0008
	}

	
}